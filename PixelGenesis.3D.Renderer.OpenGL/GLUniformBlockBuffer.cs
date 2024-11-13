using OpenTK.Graphics.OpenGL4;
using PixelGenesis._3D.Renderer.DeviceApi.Abstractions;
using System.Runtime.CompilerServices;

namespace PixelGenesis._3D.Renderer.DeviceApi.OpenGL;

public class GLUniformBlockBuffer : IUniformBlockBuffer
{
    int _id;
    public int Id => _id;

    OpenGLDeviceApi _api;

    int[] _offsets;

    public GLUniformBlockBuffer(int size, int[] offsets, BufferUsageHint hint, OpenGLDeviceApi api)
    {
        GL.GenBuffers(1, out _id);
        OpenGLDeviceApi.ThrowOnGLError();
        GL.BindBuffer(BufferTarget.UniformBuffer, _id);
        OpenGLDeviceApi.ThrowOnGLError();
        GL.BufferData(BufferTarget.UniformBuffer, size, 0, hint);


        _offsets = offsets;
        _api = api;
        _api._uniformBlockBuffers.Add(_id, this);
    }

    public unsafe void SetData<T>(T data, int index) where T : unmanaged
    {
        Bind();
        GL.BufferSubData(BufferTarget.UniformBuffer, _offsets[index], sizeof(T), (IntPtr)Unsafe.AsPointer(ref data));
        OpenGLDeviceApi.ThrowOnGLError();
    }

    public unsafe void SetData(ReadOnlySpan<byte> data, int index)
    {
        IntPtr dataPointer;
        fixed (byte* pointer = data)
            dataPointer = (IntPtr)pointer;
        Bind();
        GL.BufferSubData(BufferTarget.UniformBuffer, _offsets[index], data.Length, dataPointer);
        OpenGLDeviceApi.ThrowOnGLError();
    }

    public void Bind()
    {
        GL.BindBuffer(BufferTarget.UniformBuffer, _id);
        OpenGLDeviceApi.ThrowOnGLError();
    }

    public void Unbind()
    {
        GL.BindBuffer(BufferTarget.UniformBuffer, 0);
        OpenGLDeviceApi.ThrowOnGLError();
    }

    public void Dispose()
    {
        GL.DeleteBuffer(_id);
        OpenGLDeviceApi.ThrowOnGLError();
        _api._uniformBlockBuffers.Remove(_id);
    }
}
