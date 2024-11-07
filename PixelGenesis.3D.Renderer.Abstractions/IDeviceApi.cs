using System.Numerics;

namespace PixelGenesis._3D.Renderer.DeviceApi.Abstractions;

public interface IDeviceApi : IDisposable
{
    IVertexBuffer GetVertexBufferById(int id);
    IIndexBuffer GetIndexBufferById(int id);
    IIndexBuffer<T> GetIndexBufferById<T>(int id) where T : unmanaged, IBinaryInteger<T>
    {
        return (IIndexBuffer<T>)GetIndexBufferById(id);
    }
    IUniformBlockBuffer GetUniformBlockBufferById(int id);    
    ITexture GetTextureById(int id);
    IShaderProgram GetShaderProgramById(int id);

    public IVertexBuffer CreateVertexBuffer(int size, BufferHint bufferHint);
    public IVertexBuffer CreateVertexBuffer(ReadOnlyMemory<byte> data, BufferHint bufferHint);

    public IIndexBuffer<T> CreateIndexBuffer<T>(int lenght, BufferHint bufferHint) where T : unmanaged, IBinaryInteger<T>;
    public IIndexBuffer<T> CreateIndexBuffer<T>(ReadOnlyMemory<T> data, BufferHint bufferHint) where T : unmanaged, IBinaryInteger<T>;

    public ITexture CreateTexture(int width, int height, ReadOnlyMemory<byte> data, PGPixelFormat pixelFormat, PGInternalPixelFormat internalPixelFormat, PGPixelType pixelType);

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

    void DrawTriangles(DrawContext drawContext);    
}

public enum BufferHint
{
    Dynamic,
    Static
}