using CommunityToolkit.HighPerformance;
using PixelGenesis._3D.Common.Geometry;
using PixelGenesis._3D.Renderer.DeviceApi.Abstractions;
using PixelGenesis.ECS;
using PixelGenesis.ECS.Components;
using System.Numerics;

namespace PixelGenesis._3D.Common.Components;

public sealed partial class PerspectiveCameraComponent(Transform3DComponent transform3D) : Component
{
    public float FieldOfView = 45f;

    public float NearPlaneDistance = 0.1f;

    public float FarPlaneDistance = 1000f;

    public Texture[]? Skybox;

    public Transform3DComponent GetTransform() => transform3D;

    SkyboxRenderer? _skyboxRenderer;

    public Matrix4x4 GetProjectionMatrix(float aspectRatio)
    {
        return Matrix4x4.CreatePerspectiveFieldOfView((MathF.PI / 180) * FieldOfView, aspectRatio, NearPlaneDistance, FarPlaneDistance);
    }

    public Matrix4x4 GetViewMatrix()
    {
        var position = transform3D.Position;
        var rotation = transform3D.Rotation;
                
        var forward = Vector3.Transform(Vector3.UnitZ, rotation);
        var up = Vector3.Transform(Vector3.UnitY, rotation);

        return Matrix4x4.CreateLookAt(position, position - forward, up);
    }

    public SkyboxRenderer? GetSkyboxRenderer(IDeviceApi deviceApi)
    {
        if(Skybox is null)
        {
            return null;
        }

        if(_skyboxRenderer is not null)
        {
            return _skyboxRenderer;
        }

        _skyboxRenderer = new SkyboxRenderer(deviceApi, Skybox);
        _skyboxRenderer.Initialize();

        return _skyboxRenderer;
    }
}


public class SkyboxRenderer(IDeviceApi deviceApi, ReadOnlyMemory<Texture> textures) : IDisposable
{

    public const string SkyboxVertexShader = """
        #version 450
        layout (location = 0) in vec3 aPos;

        layout (location = 0) out vec3 texCoords;

        layout(binding = 0) uniform Details {
            mat4 projection;
            mat4 view;
        } details;

        void main()
        {
            texCoords = aPos;
            vec4 pos = details.projection * details.view * vec4(aPos, 1.0);
            gl_Position = pos.xyww;
        }  
        """;

    public const string SkyboxFragmentShader = """
        #version 450
        layout (location=0) out vec4 fragColor;

        layout (location=0) in vec3 texCoords;

        layout (binding=0) uniform samplerCube skybox;

        void main()
        {    
            fragColor = texture(skybox, texCoords);
        }
        """;

    readonly static ReadOnlyMemory<byte> VertexBytecode;
    readonly static ReadOnlyMemory<byte> FragmentBytecode;

    DrawContext drawContext = new DrawContext()
    {
        DepthFunc = PGDepthFunc.Lequal,
    };

    static SkyboxRenderer()
    {
        VertexBytecode = ShadersHelper.CompileGLSLSourceToSpirvBytecode(SkyboxVertexShader, "vert");
        FragmentBytecode = ShadersHelper.CompileGLSLSourceToSpirvBytecode(SkyboxFragmentShader, "frag");
    }

    IShaderProgram shaderProgram;
    ICubemapTexture cubemapTexture;
    IVertexBuffer vertexBuffer;
    BufferLayout layout;
    IIndexBuffer indices;
    IUniformBlockBuffer uniformBlockBuffer;

    public void Initialize()
    {
        shaderProgram = deviceApi.CreateShaderProgram(VertexBytecode, FragmentBytecode, ReadOnlyMemory<byte>.Empty, ReadOnlyMemory<byte>.Empty);
        uniformBlockBuffer = deviceApi.CreateUniformBlockBuffer<Matrix4x4, Matrix4x4>(BufferHint.Dynamic);

        if (textures.Length is not 6)
        {
            throw new InvalidOperationException("Skybox needs exactly 6 textures.");
        }

        Span<(int Width, int Height)> dimensions = stackalloc (int, int)[6];
        Span<ReadOnlyMemory<byte>> data = new ReadOnlyMemory<byte>[6];

        for (var i = 0; i < 6; i++)
        {
            var texture = textures.Span[i];
            dimensions[i] = (texture.Width, texture.Height);
            data[i] = texture.Data;
        }

        var firstTexture = textures.Span[0];

        cubemapTexture = deviceApi.CreateCubemapTexture(
            dimensions,
            data,
            firstTexture.PixelFormat,
            PGInternalPixelFormat.Rgb,
            PGPixelType.UnsignedByte);

        var mesh = MeshGenerator.SkyboxCube;

        vertexBuffer = deviceApi.CreateVertexBuffer(mesh.Vertices.AsBytes(), BufferHint.Static);
        indices = deviceApi.CreateIndexBuffer(mesh.Triangles, BufferHint.Static);

        layout = new BufferLayout();
        layout.PushFloat(3, false);


    }

    public void DrawSkybox(Matrix4x4 projection, Matrix4x4 view)
    {
        var noTranslationView = view;

        noTranslationView.Translation = Vector3.Zero;

        drawContext.Lenght = MeshGenerator.SkyboxCube.Triangles.Length;
        drawContext.VertexBuffer = vertexBuffer;
        drawContext.IndexBuffer = indices;
        drawContext.ShaderProgram = shaderProgram;
        drawContext.Layout = layout;
        
        
        uniformBlockBuffer.Bind();

        uniformBlockBuffer.SetData(projection, 0);
        uniformBlockBuffer.SetData(noTranslationView, 1);

        shaderProgram.SetUniformBlock(0, uniformBlockBuffer);
        cubemapTexture.Bind();
        cubemapTexture.SetSlot(0);

        deviceApi.DrawTriangles(drawContext);
    }

    public void Dispose()
    {
        shaderProgram.Dispose();
        cubemapTexture.Dispose();
        vertexBuffer.Dispose();
        indices.Dispose();
    }
}
