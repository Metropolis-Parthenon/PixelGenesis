//using OpenTK.Graphics.OpenGL4;
//using OpenTK.Windowing.Common;
//using OpenTK.Windowing.Desktop;
//using PixelGenesis._3D.Renderer.DeviceApi.Abstractions;
//using PixelGenesis._3D.Renderer.DeviceApi.OpenGL;
//using CommunityToolkit.HighPerformance;
//using PixelGenesis._3D.Common;
//using System.Numerics;
//using PixelGenesis._3D.Common.Components;
//using OpenTK.Windowing.GraphicsLibraryFramework;

//namespace Shader.Sandbox;

//internal class Game : GameWindow
//{
//    public Game(int width, int height, string title) : base(GameWindowSettings.Default, new NativeWindowSettings() { ClientSize = (width, height), Title = title }) { }

//    IDeviceApi deviceApi = new OpenGLDeviceApi();
//    IMesh mesh;

//    Transform3DComponent cameraTransform = new Transform3DComponent()
//    {
//        Position = new Vector3(0f, 0f, -3f),
//        Rotation = Quaternion.CreateFromYawPitchRoll(0f, 0f, 0f),
//        Scale = new Vector3(1f, 1f, 1f)
//    };

//    PerspectiveCameraComponent PerspectiveCameraComponent;

//    List<Transform3DComponent> Transforms = new List<Transform3DComponent>()
//    {
//        //new Transform3DComponent()
//        //{
//        //    Position = new Vector3(0f, 0f, 0f),
//        //    Rotation = Quaternion.CreateFromYawPitchRoll(0f, 0f, 0f),
//        //    Scale = new Vector3(1f, 1f, 1f)
//        //},
//        //new Transform3DComponent()
//        //{
//        //    Position = new Vector3(2f, 0f, 0f),
//        //    Rotation = Quaternion.CreateFromYawPitchRoll(0f, 0f, 0f),
//        //    Scale = new Vector3(1f, 1f, 1f)
//        //},
//        //new Transform3DComponent()
//        //{
//        //    Position = new Vector3(-2f, 0f, 0f),
//        //    Rotation = Quaternion.CreateFromYawPitchRoll(0f, 0f, 0f),
//        //    Scale = new Vector3(1f, 1f, 1f)
//        //}
//    };

//    Vector3[] vertices = [
//        new Vector3(-0.5f, -0.5f, -0.5f),
//        new Vector3(0.5f, -0.5f, -0.5f),
//        new Vector3(0.5f,  0.5f, -0.5f),
//        new Vector3(0.5f,  0.5f, -0.5f),
//        new Vector3(-0.5f,  0.5f, -0.5f),
//        new Vector3(-0.5f, -0.5f, -0.5f),

//        new Vector3(-0.5f, -0.5f,  0.5f),
//        new Vector3(0.5f, -0.5f,  0.5f),
//        new Vector3(0.5f,  0.5f,  0.5f),
//        new Vector3(0.5f,  0.5f,  0.5f),
//        new Vector3(-0.5f,  0.5f,  0.5f),
//        new Vector3(-0.5f, -0.5f,  0.5f),

//        new Vector3(-0.5f,  0.5f,  0.5f),
//        new Vector3(-0.5f,  0.5f, -0.5f),
//        new Vector3(-0.5f, -0.5f, -0.5f),
//        new Vector3(-0.5f, -0.5f, -0.5f),
//        new Vector3(-0.5f, -0.5f,  0.5f),
//        new Vector3(-0.5f,  0.5f,  0.5f),

//        new Vector3(0.5f,  0.5f,  0.5f),
//        new Vector3(0.5f,  0.5f, -0.5f),
//        new Vector3(0.5f, -0.5f, -0.5f),
//        new Vector3(0.5f, -0.5f, -0.5f),
//        new Vector3(0.5f, -0.5f,  0.5f),
//        new Vector3(0.5f,  0.5f,  0.5f),

//        new Vector3(-0.5f, -0.5f, -0.5f),
//        new Vector3(0.5f, -0.5f, -0.5f),
//        new Vector3(0.5f, -0.5f,  0.5f),
//        new Vector3(0.5f, -0.5f,  0.5f),
//        new Vector3(-0.5f, -0.5f,  0.5f),
//        new Vector3(-0.5f, -0.5f, -0.5f),

//        new Vector3(-0.5f,  0.5f, -0.5f),
//        new Vector3(0.5f,  0.5f, -0.5f),
//        new Vector3(0.5f,  0.5f,  0.5f),
//        new Vector3(0.5f,  0.5f,  0.5f),
//        new Vector3(-0.5f,  0.5f,  0.5f),
//        new Vector3(-0.5f,  0.5f, -0.5f),
//        ];

//    uint[] indices = [
//        0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 ,11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35
//        ];

//    IShaderProgram shaderProgram;

//    IVertexBuffer vertexBuffer;
//    VertexBufferLayout layout;
//    IIndexBuffer<uint> indexBuffer;
//    CompiledShader shader;

//    IInstanceBuffer instanceBuffer;
//    VertexBufferLayout instanceLayout;

//    IUniformBlockBuffer viewProjection;

//    DrawContext drawContext;

//    protected override void OnLoad()
//    {
//        base.OnLoad();

//        var offset = 0f;
//        for (var x = -2; x < 2; x++) 
//        { 
//            for(var y = -2; y < 2; y++)
//            {
//                var transform = new Transform3DComponent()
//                {
//                    Position = new Vector3(x * 2f + offset, y * 2f + offset, 0f),
//                    Rotation = Quaternion.CreateFromYawPitchRoll(0f, 0f, 0f),
//                    Scale = new Vector3(1f, 1f, 1f)
//                };
//                Transforms.Add(transform);
//            }
//        }

//        PerspectiveCameraComponent = new PerspectiveCameraComponent(cameraTransform);

//        mesh = new MutableMesh()
//        {
//            MutableVertices = vertices,
//            MutableTriangles = indices
//        };

//        var vao = GL.GenVertexArray();
//        GL.BindVertexArray(vao);

//        viewProjection = deviceApi.CreateUniformBlockBuffer<Matrix4x4, Matrix4x4>(BufferHint.Dynamic);

//        vertexBuffer = deviceApi.CreateVertexBuffer(mesh.Vertices.AsBytes(), BufferHint.Static);
//        layout = new VertexBufferLayout();
//        layout.PushFloat(3, false);

//        Matrix4x4[] models = Transforms.Select(t => t.GetModelMatrix()).ToArray();

//        instanceBuffer = deviceApi.CreateInstanceBuffer(models.AsMemory().AsBytes(), BufferHint.Static);
//        instanceLayout = new VertexBufferLayout();
//        instanceLayout.PushFloat(4, false);
//        instanceLayout.PushFloat(4, false);
//        instanceLayout.PushFloat(4, false);
//        instanceLayout.PushFloat(4, false);

//        indexBuffer = deviceApi.CreateIndexBuffer(mesh.Triangles, BufferHint.Static);

//        using var fileStream = File.OpenRead("shader.yaml");
//        shader = new PGGLSLShaderSource.Factory().ReadAsset("shader.yaml", fileStream).CompiledShader().Shader ?? throw new Exception("Cannot compile shader");

//        shaderProgram = deviceApi.CreateShaderProgram(shader.Vertex, shader.Fragment, shader.Tessellation, shader.Geometry);

//        drawContext = new DrawContext()
//        {
//            VertexBuffer = vertexBuffer,
//            IndexBuffer = indexBuffer,
//            ShaderProgram = shaderProgram,
//            Layout = layout,
//            Lenght = mesh.Triangles.Length,
//            EnableDepthTest = false
//        };
//    }

//    protected override void OnRenderFrame(FrameEventArgs args)
//    {
//        base.OnRenderFrame(args);

//        HandleInput((float)args.Time);

//        GL.Clear(ClearBufferMask.ColorBufferBit);

//        viewProjection.SetData(PerspectiveCameraComponent.GetViewMatrix(), 0);
//        viewProjection.SetData(PerspectiveCameraComponent.GetProjectionMatrix(Size.X / Size.Y), 1);

//        shaderProgram.SetUniformBlock(0, viewProjection);

//        deviceApi.DrawTriangles(drawContext, Transforms.Count, instanceBuffer, instanceLayout);

//        SwapBuffers();
//    }

//    void HandleInput(float deltaTime)
//    {
//        float speed = 2f;

//        if (KeyboardState.IsKeyDown(Keys.W))
//        {
//            cameraTransform.Position.Z += speed * deltaTime;
//        }

//        if (KeyboardState.IsKeyDown(Keys.S))
//        {
//            cameraTransform.Position.Z -= speed * deltaTime;
//        }

//        if (KeyboardState.IsKeyDown(Keys.A))
//        {
//            cameraTransform.Position.X += speed * deltaTime;
//        }

//        if (KeyboardState.IsKeyDown(Keys.D))
//        {
//            cameraTransform.Position.X -= speed * deltaTime;
//        }

//        if(KeyboardState.IsKeyDown(Keys.LeftShift))
//        {
//            cameraTransform.Position.Y += speed * deltaTime;
//        }

//        if(KeyboardState.IsKeyDown(Keys.LeftControl))
//        {
//            cameraTransform.Position.Y -= speed * deltaTime;
//        }

//        if(MouseState.IsButtonDown(MouseButton.Right))
//        {
            
//        }

//    }

//    protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
//    {
//        base.OnFramebufferResize(e);
//        GL.Viewport(0, 0, e.Width, e.Height);
//    }

//}
