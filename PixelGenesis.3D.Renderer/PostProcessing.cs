using CommunityToolkit.HighPerformance;
using PixelGenesis._3D.Renderer.DeviceApi.Abstractions;

namespace PixelGenesis._3D.Renderer;

internal class PostProcessing(IDeviceApi deviceApi) : IDisposable
{

    readonly static float[] quadVertices = new float[]{ // vertex attributes for a quad that fills the entire screen in Normalized Device Coordinates.
    // positions   // texCoords
    -1.0f,  1.0f,  0.0f, 1.0f,
    -1.0f, -1.0f,  0.0f, 0.0f,
     1.0f, -1.0f,  1.0f, 0.0f,

    -1.0f,  1.0f,  0.0f, 1.0f,
     1.0f, -1.0f,  1.0f, 0.0f,
     1.0f,  1.0f,  1.0f, 1.0f
};
    readonly static uint[] indexes = [0, 1, 2, 3, 4, 5, 6];

    const string VertexShader = """
    #version 450
    layout (location = 0) in vec2 aPos;
    layout (location = 1) in vec2 aTexCoords;

    layout (location = 0) out vec2 TexCoords;

    void main()
    {
        gl_Position = vec4(aPos.x, aPos.y, 0.0, 1.0); 
        TexCoords = aTexCoords;
    }  
    """;

    const string FragmentShader = """
    #version 450
    layout (location = 0) out vec4 FragColor;

    layout (location = 0) in vec2 TexCoords;

    layout (binding = 0) uniform sampler2D screenTexture;

    const float offset = 1.0 / 300.0;

    void main()
    { 
        vec2 offsets[9] = vec2[](
        vec2(-offset,  offset), // top-left
        vec2( 0.0f,    offset), // top-center
        vec2( offset,  offset), // top-right
        vec2(-offset,  0.0f),   // center-left
        vec2( 0.0f,    0.0f),   // center-center
        vec2( offset,  0.0f),   // center-right
        vec2(-offset, -offset), // bottom-left
        vec2( 0.0f,   -offset), // bottom-center
        vec2( offset, -offset)  // bottom-right    
        );

        float kernel[9] = float[](
            0, 0, 0,
            0,  1, 0,
            0, 0, 0
        );

        vec3 sampleTex[9];
        for(int i = 0; i < 9; i++)
        {
            sampleTex[i] = vec3(texture(screenTexture, TexCoords.st + offsets[i]));
        }
        vec3 col = vec3(0.0);
        for(int i = 0; i < 9; i++)
            col += sampleTex[i] * kernel[i];

        FragColor = vec4(col, 1.0);
    }
    """;

    IShaderProgram shaderProgram;
    IVertexBuffer vertexBuffer;
    BufferLayout bufferLayout = new BufferLayout();
    IIndexBuffer indexBuffer;
    DrawContext drawContext;

    public void Initialize()
    {
        vertexBuffer = deviceApi.CreateVertexBuffer(quadVertices.AsMemory().AsBytes(), BufferHint.Static);
        indexBuffer = deviceApi.CreateIndexBuffer<uint>(indexes, BufferHint.Static);

        bufferLayout.PushFloat(2, false);
        bufferLayout.PushFloat(2, false);

        var vertexShaderBytecode = ShadersHelper.CompileGLSLSourceToSpirvBytecode(VertexShader, "vert");
        var fragmentShaderBytecode = ShadersHelper.CompileGLSLSourceToSpirvBytecode(FragmentShader, "frag");

        shaderProgram = deviceApi.CreateShaderProgram(vertexShaderBytecode, fragmentShaderBytecode, Memory<byte>.Empty, Memory<byte>.Empty);

        drawContext = new DrawContext()
        {
            VertexBuffer = vertexBuffer,
            IndexBuffer = indexBuffer,
            ShaderProgram = shaderProgram,
            Layout = bufferLayout,
            EnableDepthTest = false,
            EnableBlend = false,
            Lenght = indexes.Length
        };
    }    

    public void Draw(ITexture texture)
    {
        texture.SetSlot(0);
        texture.Bind();
        deviceApi.DrawTriangles(drawContext);
    }

    public void Dispose()
    {
        shaderProgram.Dispose();
        vertexBuffer.Dispose();
        indexBuffer.Dispose();
    }
}
