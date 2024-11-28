using System.Drawing;

namespace PixelGenesis._3D.Renderer.DeviceApi.Abstractions;

public class DrawContext
{
    public IVertexBuffer VertexBuffer;
    public BufferLayout Layout;    
    public IIndexBuffer IndexBuffer;
    public IShaderProgram ShaderProgram;
    public int Lenght;
    public int Offset;
    public int? BaseVertex;
    public bool EnableBlend = true;
    public PGBlendEquation BlendEquation = PGBlendEquation.Add;
    public PGBlendingFactor BlendSFactor = PGBlendingFactor.SrcAlpha;
    public PGBlendingFactor BlendDFactor = PGBlendingFactor.OneMinusSrcAlpha;
    public bool EnableDepthTest = true;
    public bool EnableCullFace = false;
    public bool EnableScissorTest = false;
    public bool DepthMask = true;
    public PGDepthFunc DepthFunc = PGDepthFunc.Less;
    public Rectangle ScissorRect;
}

public enum PGDepthFunc
{
    Never = 512,
    Less,
    Equal,
    Lequal,
    Greater,
    Notequal,
    Gequal,
    Always
}

public enum PGBlendEquation
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