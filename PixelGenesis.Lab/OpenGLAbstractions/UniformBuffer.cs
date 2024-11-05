using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PixelGenesis.Lab.OpenGLAbstractions;

public class UniformBuffer
{
    public int _rendererID;

    int[] offsets;

    public unsafe UniformBuffer(int size, int[] offsets)
    {
        GL.GenBuffers(1, out _rendererID);
        GL.BindBuffer(BufferTarget.UniformBuffer, _rendererID);
        GL.BufferData(BufferTarget.UniformBuffer, size, 0, BufferUsageHint.DynamicDraw);

        this.offsets = offsets;
    }

    public unsafe static UniformBuffer Create<T>() 
        where T : unmanaged
    {
        return new UniformBuffer(sizeof(T), [0]);
    }

    public unsafe static UniformBuffer Create<T, T1>() 
        where T : unmanaged
        where T1 : unmanaged
    {
        return new UniformBuffer(sizeof(T) + sizeof(T1), [0, sizeof(T)]);
    }

    public unsafe static UniformBuffer Create<T, T1, T2>()
    where T : unmanaged
    where T1 : unmanaged
    where T2 : unmanaged
    {
        return new UniformBuffer(sizeof(T) + sizeof(T1) + sizeof(T2), [0, sizeof(T), sizeof(T)+sizeof(T1)]);
    }

    public unsafe void SetData<T>(T data, int index) where T : unmanaged
    {
        GL.BindBuffer(BufferTarget.UniformBuffer, _rendererID);
        GL.BufferSubData(BufferTarget.UniformBuffer, offsets[index], sizeof(T), (IntPtr)Unsafe.AsPointer(ref data));
    }

    public void Bind()
    {
        GL.BindBuffer(BufferTarget.UniformBuffer, _rendererID);
    }

    public void Unbind()
    {
        GL.BindBuffer(BufferTarget.UniformBuffer, 0);
    }

    public void Dispose()
    {
        GL.DeleteBuffer(_rendererID);
    }
}
