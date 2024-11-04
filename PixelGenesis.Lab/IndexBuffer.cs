using OpenTK.Graphics.OpenGL4;
using System.Runtime.CompilerServices;

namespace PixelGenesis.Lab;

internal class IndexBuffer : IDisposable
{
    uint _rendererID;
    int _count;

    public int Count => _count;

    public unsafe IndexBuffer(Memory<uint> data)
    {
        var dataPointer = Unsafe.AsPointer(ref data.Span.GetPinnableReference());

        _count = data.Length;

        GL.GenBuffers(1, out _rendererID);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _rendererID);
        GL.BufferData(BufferTarget.ElementArrayBuffer, data.Length * sizeof(uint), (IntPtr)dataPointer, BufferUsageHint.StaticDraw);
    }

    public void Bind()
    {
        GL.BindBuffer(BufferTarget.ArrayBuffer, _rendererID);
    }

    public void Unbind()
    {
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
    }

    public void Dispose()
    {
        GL.DeleteBuffer(_rendererID);
    }
}
