using OpenTK.Graphics.OpenGL4;
using System.Runtime.InteropServices;

namespace PixelGenesis.Lab.OpenGLAbstractions;

struct LayoutElement
{
    public int Count;
    public int Size;
    public VertexAttribPointerType Type;
    public bool Normalized;
}
internal class VertexBufferLayout
{
    List<LayoutElement> elements = new List<LayoutElement>();

    int stride = 0;

    public int Stride => stride;
    public ReadOnlySpan<LayoutElement> Elements => CollectionsMarshal.AsSpan(elements);

    public void PushFloat(int count)
    {
        var size = sizeof(float);
        elements.Add(new LayoutElement { Count = count, Type = VertexAttribPointerType.Float, Normalized = false, Size = size });
        stride += size * count;
    }

    public void PushUInt(int count)
    {
        var size = sizeof(uint);
        elements.Add(new LayoutElement { Count = count, Type = VertexAttribPointerType.UnsignedInt, Normalized = false, Size = size });
        stride += size * count;
    }

    public void PushByte(int count)
    {
        var size = sizeof(byte);
        elements.Add(new LayoutElement { Count = count, Type = VertexAttribPointerType.UnsignedByte, Normalized = true, Size = size });
        stride += size * count;
    }
}
