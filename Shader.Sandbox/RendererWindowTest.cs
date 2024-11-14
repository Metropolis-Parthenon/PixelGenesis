using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using PixelGenesis._3D.Common;
using PixelGenesis._3D.Renderer.DeviceApi.Abstractions;
using PixelGenesis._3D.Renderer.DeviceApi.OpenGL;
using System.Numerics;
using OpenTK.Graphics.OpenGL4;
using PixelGenesis._3D.Renderer;
using PixelGenesis.ECS;
using PixelGenesis._3D.Common.Components;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Shader.Sandbox;


internal class RendererWindowTest : GameWindow, IPGWindow
{
    public RendererWindowTest(int width, int height, string title) : base(GameWindowSettings.Default, new NativeWindowSettings() { ClientSize = (width, height), Title = title }) { }

    IDeviceApi deviceApi = new OpenGLDeviceApi();
    PG3DRenderer renderer;
    EntityManager entityManager;

    PerspectiveCameraComponent PerspectiveCameraComponent;

    Material cubeMaterial = new Material(
        "shader.yaml",
        new PGGLSLShaderSource.Factory().ReadAsset("shader.yaml", File.OpenRead("shader.yaml")).CompiledShader().Shader ?? throw new Exception("Cannot Compile Shader")
        );

    IMesh cubeMesh = new MutableMesh() 
    { 
        MutableVertices = new Memory<Vector3>([
        new Vector3(-0.5f, -0.5f, -0.5f),
        new Vector3(0.5f, -0.5f, -0.5f),
        new Vector3(0.5f,  0.5f, -0.5f),
        new Vector3(0.5f,  0.5f, -0.5f),
        new Vector3(-0.5f,  0.5f, -0.5f),
        new Vector3(-0.5f, -0.5f, -0.5f),

        new Vector3(-0.5f, -0.5f,  0.5f),
        new Vector3(0.5f, -0.5f,  0.5f),
        new Vector3(0.5f,  0.5f,  0.5f),
        new Vector3(0.5f,  0.5f,  0.5f),
        new Vector3(-0.5f,  0.5f,  0.5f),
        new Vector3(-0.5f, -0.5f,  0.5f),

        new Vector3(-0.5f,  0.5f,  0.5f),
        new Vector3(-0.5f,  0.5f, -0.5f),
        new Vector3(-0.5f, -0.5f, -0.5f),
        new Vector3(-0.5f, -0.5f, -0.5f),
        new Vector3(-0.5f, -0.5f,  0.5f),
        new Vector3(-0.5f,  0.5f,  0.5f),

        new Vector3(0.5f,  0.5f,  0.5f),
        new Vector3(0.5f,  0.5f, -0.5f),
        new Vector3(0.5f, -0.5f, -0.5f),
        new Vector3(0.5f, -0.5f, -0.5f),
        new Vector3(0.5f, -0.5f,  0.5f),
        new Vector3(0.5f,  0.5f,  0.5f),

        new Vector3(-0.5f, -0.5f, -0.5f),
        new Vector3(0.5f, -0.5f, -0.5f),
        new Vector3(0.5f, -0.5f,  0.5f),
        new Vector3(0.5f, -0.5f,  0.5f),
        new Vector3(-0.5f, -0.5f,  0.5f),
        new Vector3(-0.5f, -0.5f, -0.5f),

        new Vector3(-0.5f,  0.5f, -0.5f),
        new Vector3(0.5f,  0.5f, -0.5f),
        new Vector3(0.5f,  0.5f,  0.5f),
        new Vector3(0.5f,  0.5f,  0.5f),
        new Vector3(-0.5f,  0.5f,  0.5f),
        new Vector3(-0.5f,  0.5f, -0.5f),
        ]),

        MutableTriangles = new Memory<uint>([
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 ,11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35
        ]),
    };

    public float Width => Size.X;

    public float Height => Size.Y;

    protected override void OnLoad()
    {
        base.OnLoad();
        ComponentInitializer.Initialize();

        entityManager = new EntityManager();
        renderer = new PG3DRenderer(deviceApi, this, entityManager);

        // setup scene

        //cube
        var cube = entityManager.Create("Cube");
        var meshRendererComponent = cube.AddComponent<MeshRendererComponent>();
        meshRendererComponent.Mesh = cubeMesh;
        meshRendererComponent.Material = cubeMaterial;
        cube.GetComponent<Transform3DComponent>().Rotation = Quaternion.CreateFromYawPitchRoll((MathF.PI / 180f) * 45f, 0, 0f);

        var offset = 0f;
        for (var x = -2; x < 2; x++)
        {
            for (var y = -2; y < 2; y++)
            {                
                var clone = entityManager.Clone(cube, $"Cube{x}{y}");
                clone.GetComponent<Transform3DComponent>().Position = new Vector3(x * 2f + offset, y * 2f + offset, 0f);
            }
        }

        //camera
        var camera = entityManager.Create("Camera");
        PerspectiveCameraComponent = camera.AddComponent<PerspectiveCameraComponent>();
        camera.GetComponent<Transform3DComponent>().Position = new Vector3(0, 0, -5);






        var vao = GL.GenVertexArray();
        GL.BindVertexArray(vao);
        renderer.Initialize();

        GL.ClearColor(1f, 1f, 1f, 1f);

    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        HandleInput((float)args.Time);

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        renderer.Update();
        renderer.Render();

        SwapBuffers();
    }

    void HandleInput(float deltaTime)
    {
        float speed = 2f;
        var cameraTransform = PerspectiveCameraComponent.Entity.GetComponent<Transform3DComponent>();
        if (KeyboardState.IsKeyDown(Keys.W))
        {
            cameraTransform.Position.Z += speed * deltaTime;
        }

        if (KeyboardState.IsKeyDown(Keys.S))
        {
            cameraTransform.Position.Z -= speed * deltaTime;
        }

        if (KeyboardState.IsKeyDown(Keys.A))
        {
            cameraTransform.Position.X += speed * deltaTime;
        }

        if (KeyboardState.IsKeyDown(Keys.D))
        {
            cameraTransform.Position.X -= speed * deltaTime;
        }

        if (KeyboardState.IsKeyDown(Keys.LeftShift))
        {
            cameraTransform.Position.Y += speed * deltaTime;
        }

        if (KeyboardState.IsKeyDown(Keys.LeftControl))
        {
            cameraTransform.Position.Y -= speed * deltaTime;
        }

    }

    protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
    {
        base.OnFramebufferResize(e);
        GL.Viewport(0, 0, e.Width, e.Height);
    }

}
