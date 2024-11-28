using System.Runtime.InteropServices;

namespace PixelGenesis._3D.Renderer.DeviceApi.Abstractions;

public class BufferLayout
{
    public struct LayoutElement
    {
        public int Count;
        public int Size;
        public ShaderDataType Type;
        public bool Normalized;
    }

    List<LayoutElement> _elements = new List<LayoutElement>();

    public Span<LayoutElement> Elements => CollectionsMarshal.AsSpan(_elements);

    public int Stride { get; private set; } = 0;

    public void PushFloat(int count, bool normalize)
    {
        var size = sizeof(float);
        _elements.Add(new LayoutElement { Count = count, Type = ShaderDataType.Float, Normalized = normalize, Size = size });
        Stride += size * count;
    }

    public void PushUInt(int count, bool normalize)
    {
        var size = sizeof(uint);
        _elements.Add(new LayoutElement { Count = count, Type = ShaderDataType.Uint, Normalized = normalize, Size = size });
        Stride += size * count;
    }

    public void PushByte(int count, bool normalize)
    {
        var size = sizeof(byte);
        _elements.Add(new LayoutElement { Count = count, Type = ShaderDataType.Byte, Normalized = normalize, Size = size });
        Stride += size * count;
    }
    public void Clear()
    {
        _elements.Clear();
    }

}

public enum ShaderDataType
{    
    Float,
    Uint,
    Byte
}