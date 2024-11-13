using System.Drawing;

namespace PixelGenesis._3D.Renderer.DeviceApi.Abstractions;

public class DrawContext
{
    public IVertexBuffer VertexBuffer;
    public VertexBufferLayout Layout;    
    public IIndexBuffer IndexBuffer;
    public IShaderProgram ShaderProgram;
    public int Lenght;
    public int Offset;
    public int? BaseVertex;
    public bool EnableBlend = true;
    public GPBlendEquation BlendEquation = GPBlendEquation.Add;
    public PGBlendingFactor BlendSFactor = PGBlendingFactor.SrcAlpha;
    public PGBlendingFactor BlendDFactor = PGBlendingFactor.OneMinusSrcAlpha;
    public bool EnableDepthTest = false;
    public bool EnableCullFace = false;
    public bool EnableScissorTest = false;
    public Rectangle ScissorRect;
}

public enum GPBlendEquation
{
    Add = 32774,
    Subtract = 32778,
    ReverseSubtract = 32779,
    Min = 32775,
    Max = 32776
}

public enum PGBlendingFactor
{
    Zero = 0,
    SrcColor = 768,
    OneMinusSrcColor = 769,
    SrcAlpha = 770,
    OneMinusSrcAlpha = 771,
    DstAlpha = 772,
    OneMinusDstAlpha = 773,
    DstColor = 774,
    OneMinusDstColor = 775,
    SrcAlphaSaturate = 776,
    ConstantColor = 32769,
    OneMinusConstantColor = 32770,
    ConstantAlpha = 32771,
    OneMinusConstantAlpha = 32772
}