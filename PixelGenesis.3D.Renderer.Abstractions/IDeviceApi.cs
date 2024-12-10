using System.Numerics;

namespace PixelGenesis._3D.Renderer.DeviceApi.Abstractions;

public interface IDeviceApi : IDisposable
{
    void ClearColor(Vector4 color);
    void Clear(PGClearBufferMask mask, DrawContext context);
    void Viewport(int x, int y, int width, int height);
    void BindFrameBuffer(int id);
    IVertexBuffer GetVertexBufferById(int id);
    IInstanceBuffer GetInstanceBufferById(int id);
    IIndexBuffer GetIndexBufferById(int id);
    IIndexBuffer<T> GetIndexBufferById<T>(int id) where T : unmanaged, IBinaryInteger<T>
    {
        return (IIndexBuffer<T>)GetIndexBufferById(id);
    }
    IUniformBlockBuffer GetUniformBlockBufferById(int id);    
    ITexture GetTextureById(int id);
    IShaderProgram GetShaderProgramById(int id);
    IFrameBuffer GetFrameBufferById(int id);
    ICubemapTexture GetCubemapTextureById(int id);

    public IVertexBuffer CreateVertexBuffer(int size, BufferHint bufferHint);    
    public IVertexBuffer CreateVertexBuffer(ReadOnlyMemory<byte> data, BufferHint bufferHint);

    public IInstanceBuffer CreateInstanceBuffer(int size, BufferHint bufferHint);
    public IInstanceBuffer CreateInstanceBuffer(ReadOnlyMemory<byte> data, BufferHint bufferHint);

    public IIndexBuffer<T> CreateIndexBuffer<T>(int lenght, BufferHint bufferHint) where T : unmanaged, IBinaryInteger<T>;
    public IIndexBuffer<T> CreateIndexBuffer<T>(ReadOnlyMemory<T> data, BufferHint bufferHint) where T : unmanaged, IBinaryInteger<T>;

    public ITexture CreateTexture(int width, int height, ReadOnlyMemory<byte> data, PGPixelFormat pixelFormat, PGInternalPixelFormat internalPixelFormat, PGPixelType pixelType);

    public ICubemapTexture CreateCubemapTexture(ReadOnlySpan<(int Width, int Height)> dimensions ,ReadOnlySpan<ReadOnlyMemory<byte>> data, PGPixelFormat pixelFormat, PGInternalPixelFormat internalPixelFormat, PGPixelType pixelType);


    public IUniformBlockBuffer CreateUniformBlockBuffer(int[] uniformSizes, BufferHint hint);

    public unsafe IUniformBlockBuffer CreateUniformBlockBuffer<T>(BufferHint hint)
        where T : unmanaged
        {
            return CreateUniformBlockBuffer([sizeof(T)], hint);
        }

    public unsafe IUniformBlockBuffer CreateUniformBlockBuffer<T, T1>(BufferHint hint)
        where T : unmanaged
        where T1 : unmanaged
        {
            return CreateUniformBlockBuffer([sizeof(T), sizeof(T1)], hint);
        }

    public unsafe IUniformBlockBuffer CreateUniformBlockBuffer<T, T1, T2>(BufferHint hint)
        where T : unmanaged
        where T1 : unmanaged
        where T2 : unmanaged
        {
            return CreateUniformBlockBuffer([sizeof(T), sizeof(T1), sizeof(T) + sizeof(T1)], hint);
        }

    public IShaderProgram CreateShaderProgram(
        ReadOnlyMemory<byte> vertexSpv,
        ReadOnlyMemory<byte> fragmentSpv,
        ReadOnlyMemory<byte> tessellationSpv,
        ReadOnlyMemory<byte> geometrySpv);

    public IFrameBuffer CreateFrameBuffer(int width, int height);

    void DrawTriangles(DrawContext drawContext);

    void DrawTriangles(DrawContext drawContext, int instanceCount, IInstanceBuffer instanceBuffer, BufferLayout layout);
}

public enum BufferHint
{
    Dynamic,
    Static
}

[Flags]
public enum PGClearBufferMask
{
    //
    // Summary:
    //     [requires: v1.0 or KHR_context_flush_control] Original was GL_NONE = 0
    None = 0,
    //
    // Summary:
    //     [requires: v1.0] Original was GL_DEPTH_BUFFER_BIT = 0x00000100
    DepthBufferBit = 0x100,
    //
    // Summary:
    //     Original was GL_ACCUM_BUFFER_BIT = 0x00000200
    AccumBufferBit = 0x200,
    //
    // Summary:
    //     [requires: v1.0] Original was GL_STENCIL_BUFFER_BIT = 0x00000400
    StencilBufferBit = 0x400,
    //
    // Summary:
    //     [requires: v1.0] Original was GL_COLOR_BUFFER_BIT = 0x00004000
    ColorBufferBit = 0x4000,
    //
    // Summary:
    //     Original was GL_COVERAGE_BUFFER_BIT_NV = 0x00008000
    CoverageBufferBitNv = 0x8000
}