using OpenTK.Graphics.OpenGL4;
using System.Runtime.CompilerServices;

namespace PixelGenesis.Lab.OpenGLAbstractions;

internal class VertexBuffer : IDisposable
{
    uint _rendererID;

    public unsafe VertexBuffer(Memory<byte> data)
    {
        var dataPointer = Unsafe.AsPointer(ref data.Span.GetPinnableReference());

        GL.GenBuffers(1, out _rendererID);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _rendererID);
        GL.BufferData(BufferTarget.ArrayBuffer, data.Length, (nint)dataPointer, BufferUsageHint.StaticDraw);
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
