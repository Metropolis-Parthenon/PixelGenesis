using OpenTK.Graphics.OpenGL4;
using PixelGenesis._3D.Renderer.DeviceApi.Abstractions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace PixelGenesis._3D.Renderer.DeviceApi.OpenGL;

internal class GLIndexBuffer<T> : IGLIndexBuffer, IIndexBuffer<T> where T : unmanaged, IBinaryInteger<T>
{
    int _id;
    public int Id => _id;

    public int Lenght { get; }

    public int Size { get; }

    public int ElementSize {  get; }

    public DrawElementsType ElementsType { get; }

    OpenGLDeviceApi _api;

    public unsafe GLIndexBuffer(int lenght, BufferUsageHint hint, OpenGLDeviceApi api)
    {        
        _api = api;

        Lenght = lenght;
        ElementsType = GetDrawElements();

        ElementSize = sizeof(T);
        Size = lenght * ElementSize;

        GL.GenBuffers(1, out _id);
        OpenGLDeviceApi.ThrowOnGLError();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _id);
        OpenGLDeviceApi.ThrowOnGLError();
        GL.BufferData(BufferTarget.ElementArrayBuffer, Size, 0, hint);
        OpenGLDeviceApi.ThrowOnGLError();

        api._indexBuffers.Add(_id, this);
    }

    public unsafe GLIndexBuffer(ReadOnlyMemory<T> data, BufferUsageHint hint, OpenGLDeviceApi api)
    {
        _api = api;

        IntPtr dataPointer;
        fixed(T* pointer = data.Span)
            dataPointer = (IntPtr)pointer;

        Lenght = data.Length;

        GL.GenBuffers(1, out _id);
        OpenGLDeviceApi.ThrowOnGLError();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _id);
        OpenGLDeviceApi.ThrowOnGLError();
        GL.BufferData(BufferTarget.ElementArrayBuffer, data.Length * sizeof(T), dataPointer, hint);
        OpenGLDeviceApi.ThrowOnGLError();
    }

    public unsafe void SetData(int offset, ReadOnlySpan<T> data)
    {
        Bind();
        IntPtr dataPointer;
        fixed (T* pointer = data)
            dataPointer = (IntPtr)pointer;

        GL.BufferSubData(BufferTarget.ElementArrayBuffer, offset * ElementSize, data.Length * ElementSize, dataPointer);
        OpenGLDeviceApi.ThrowOnGLError();
    }

    public void Bind()
    {
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _id);
        OpenGLDeviceApi.ThrowOnGLError();
    }

    public void Unbind()
    {
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        OpenGLDeviceApi.ThrowOnGLError();
    }

    public void Dispose()
    {
        GL.DeleteBuffer(_id);
        OpenGLDeviceApi.ThrowOnGLError();
        _api._indexBuffers.Remove(_id);
    }

    DrawElementsType GetDrawElements()
    {
        return typeof(T) switch
        {
            Type t when t == typeof(byte) => DrawElementsType.UnsignedByte,
            Type t when t == typeof(ushort) => DrawElementsType.UnsignedShort,
            Type t when t == typeof(uint) => DrawElementsType.UnsignedInt,
            _ => throw new InvalidOperationException("Index buffers can only be of type byte, ushort or uint")
        };
    }

}

internal interface IGLIndexBuffer : IIndexBuffer
{
    public int Lenght { get; }
    public int Size { get; }
    public int ElementSize { get; }
    DrawElementsType ElementsType { get; }
}
