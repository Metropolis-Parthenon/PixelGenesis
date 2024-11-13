using OpenTK.Graphics.OpenGL4;
using PixelGenesis._3D.Renderer.DeviceApi.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelGenesis._3D.Renderer.DeviceApi.OpenGL;

internal sealed class GLInstanceBuffer : IInstanceBuffer
{
    OpenGLDeviceApi _api;
    int _id;
    public int Id => _id;

    public unsafe GLInstanceBuffer(ReadOnlyMemory<byte> data, BufferUsageHint hint, OpenGLDeviceApi api)
    {
        IntPtr dataPointer;
        fixed (byte* pointer = data.Span)
            dataPointer = (IntPtr)pointer;

        GL.GenBuffers(1, out _id);
        OpenGLDeviceApi.ThrowOnGLError();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _id);
        OpenGLDeviceApi.ThrowOnGLError();
        GL.BufferData(BufferTarget.ArrayBuffer, data.Length, dataPointer, hint);
        OpenGLDeviceApi.ThrowOnGLError();
        _api = api;
        _api._instanceBuffers.Add(_id, this);
    }

    public GLInstanceBuffer(int size, BufferUsageHint hint, OpenGLDeviceApi api)
    {
        GL.GenBuffers(1, out _id);
        OpenGLDeviceApi.ThrowOnGLError();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _id);
        OpenGLDeviceApi.ThrowOnGLError();
        GL.BufferData(BufferTarget.ArrayBuffer, size, 0, hint);
        OpenGLDeviceApi.ThrowOnGLError();

        _api = api;
        _api._instanceBuffers.Add(_id, this);
    }

    public unsafe void SetData(int offset, ReadOnlySpan<byte> data)
    {
        Bind();
        IntPtr dataPointer;
        fixed (byte* pointer = data)
            dataPointer = (IntPtr)pointer;

        GL.BufferSubData(BufferTarget.ArrayBuffer, offset, data.Length, dataPointer);
        OpenGLDeviceApi.ThrowOnGLError();
    }

    public void Bind()
    {
        GL.BindBuffer(BufferTarget.ArrayBuffer, _id);
        OpenGLDeviceApi.ThrowOnGLError();
    }
        
    public void Unbind()
    {
        GL.BindBuffer(BufferTarget.ArrayBuffer, _id);
        OpenGLDeviceApi.ThrowOnGLError();
    }

    public void Dispose()
    {
        GL.DeleteBuffer(_id);
        OpenGLDeviceApi.ThrowOnGLError();
        _api._instanceBuffers.Remove(_id);
    }

}
