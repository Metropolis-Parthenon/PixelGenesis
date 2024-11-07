﻿using OpenTK.Graphics.OpenGL4;
using PixelGenesis._3D.Renderer.DeviceApi.Abstractions;
using System.Numerics;

namespace PixelGenesis._3D.Renderer.DeviceApi.OpenGL;

public class OpenGLDeviceApi : IDeviceApi
{
    internal Dictionary<int, IVertexBuffer> _vertexBuffers = new Dictionary<int, IVertexBuffer>();
    internal Dictionary<int, IIndexBuffer> _indexBuffers = new Dictionary<int, IIndexBuffer>();
    internal Dictionary<int, IUniformBlockBuffer> _uniformBlockBuffers = new Dictionary<int, IUniformBlockBuffer>();
    internal Dictionary<int, ITexture> _textures = new Dictionary<int, ITexture>();
    internal Dictionary<int, IShaderProgram> _shaderPrograms = new Dictionary<int, IShaderProgram>();

    public IIndexBuffer<T> CreateIndexBuffer<T>(int lenght, BufferHint bufferHint) where T : unmanaged, IBinaryInteger<T>
    {
        return new GLIndexBuffer<T>(lenght, GetUsageHint(bufferHint), this);
    }

    public IIndexBuffer<T> CreateIndexBuffer<T>(ReadOnlyMemory<T> data, BufferHint bufferHint) where T : unmanaged, IBinaryInteger<T>
    {
        return new GLIndexBuffer<T>(data, GetUsageHint(bufferHint), this);
    }

    public IShaderProgram CreateShaderProgram(
        ReadOnlyMemory<byte> vertexSpv, 
        ReadOnlyMemory<byte> fragmentSpv, 
        ReadOnlyMemory<byte> tessellationSpv,
        ReadOnlyMemory<byte> geometrySpv)
    {
        return new GLShaderProgram(vertexSpv, fragmentSpv, tessellationSpv, geometrySpv, this);
    }

    public ITexture CreateTexture(int width, int height, ReadOnlyMemory<byte> data, PGPixelFormat pixelFormat, PGInternalPixelFormat internalPixelFormat, PGPixelType pixelType)
    {
        var glPixelFormat = pixelFormat switch
        {
            PGPixelFormat.Rgba => PixelFormat.Rgba,
            PGPixelFormat.Bgra => PixelFormat.Bgra,
            _ => throw new NotImplementedException()
        };

        var glInternalPixelFormat = internalPixelFormat switch
        {
            PGInternalPixelFormat.Rgba => PixelInternalFormat.Rgba,
            PGInternalPixelFormat.Rgba8 => PixelInternalFormat.Rgba8,
            _ => throw new NotImplementedException()
        };

        var glPixelType = pixelType switch
        {
            PGPixelType.UnsignedByte => PixelType.UnsignedByte,
            _ => throw new NotImplementedException()
        };

        return new GLTexture(width, height, data, glPixelFormat, glInternalPixelFormat, glPixelType, this);
    }

    public IUniformBlockBuffer CreateUniformBlockBuffer(int[] uniformSizes, BufferHint hint)
    {
        var sum = 0;
        var offsets = new int[uniformSizes.Length];
        var currentOffset = 0;
        for(int i = 0; i < uniformSizes.Length; i++)
        {
            sum += uniformSizes[i];
            offsets[i] = currentOffset;
            currentOffset += uniformSizes[i];
        }

        return new GLUniformBlockBuffer(sum, offsets, GetUsageHint(hint), this);
    }

    public IVertexBuffer CreateVertexBuffer(int size, BufferHint bufferHint)
    {
        return new GLVertexBuffer(size, GetUsageHint(bufferHint), this);
    }

    public IVertexBuffer CreateVertexBuffer(ReadOnlyMemory<byte> data, BufferHint bufferHint)
    {
        return new GLVertexBuffer(data, GetUsageHint(bufferHint), this);
    }

    public void DrawTriangles(DrawContext drawContext)
    {
        drawContext.ShaderProgram.Bind();
        drawContext.IndexBuffer.Bind();
        drawContext.VertexBuffer.Bind();
        SetVertexLayout(drawContext.Layout);

        if(drawContext.EnableBlend)
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendEquation((BlendEquationMode)drawContext.BlendEquation);
            GL.BlendFunc((BlendingFactor)drawContext.BlendSFactor, (BlendingFactor)drawContext.BlendDFactor);
        }
        else
        {
            GL.Disable(EnableCap.Blend);
        }

        if (drawContext.EnableDepthTest)
        {
            GL.Enable(EnableCap.DepthTest);
        }
        else
        {
            GL.Disable(EnableCap.DepthTest);
        }

        if (drawContext.EnableCullFace)
        {
            GL.Enable(EnableCap.CullFace);
        }
        else
        {
            GL.Disable(EnableCap.CullFace);
        }

        if (drawContext.EnableScissorTest)
        {
            GL.Enable(EnableCap.ScissorTest);
            GL.Scissor(drawContext.ScissorRect.X, drawContext.ScissorRect.Y, drawContext.ScissorRect.Width, drawContext.ScissorRect.Height);
        }
        else
        {
            GL.Disable(EnableCap.ScissorTest);
        }

        var glIndexBuffer = (IGLIndexBuffer)drawContext.IndexBuffer;

        if(drawContext.BaseVertex is null)
        {
            GL.DrawElements(PrimitiveType.Triangles, drawContext.Lenght, glIndexBuffer.ElementsType, drawContext.Offset * glIndexBuffer.ElementSize);
        }
        else
        {
            GL.DrawElementsBaseVertex(PrimitiveType.Triangles, drawContext.Lenght, glIndexBuffer.ElementsType, drawContext.Offset * glIndexBuffer.ElementSize, drawContext.BaseVertex.Value);            
        }
    }

    static void SetVertexLayout(VertexBufferLayout layout)
    {
        var elements = layout.Elements;

        int offset = 0;

        for (int i = 0; i < elements.Length; i++)
        {
            var element = elements[i];            
            GL.VertexAttribPointer(i, element.Count, GetPointerType(element.Type), element.Normalized, layout.Stride, offset);
            ThrowOnGLError();
            GL.EnableVertexAttribArray((uint)i);
            ThrowOnGLError();
            offset += element.Count * element.Size;
        }
    }

    public IVertexBuffer GetVertexBufferById(int id)
    {
        return _vertexBuffers[id];
    }

    public IIndexBuffer GetIndexBufferById(int id)
    {
        return _indexBuffers[id];
    }

    public IUniformBlockBuffer GetUniformBlockBufferById(int id)
    {
        return _uniformBlockBuffers[id];
    }

    public ITexture GetTextureById(int id)
    {
        return _textures[id];
    }

    public IShaderProgram GetShaderProgramById(int id)
    {
        return _shaderPrograms[id];
    }

    public void Dispose()
    {
        foreach(var obj in _vertexBuffers.Values)
        {
            obj.Dispose();
        }

        foreach(var obj in _indexBuffers.Values)
        {
            obj.Dispose();
        }

        foreach(var obj in _uniformBlockBuffers.Values)
        {
            obj.Dispose();
        }

        foreach(var obj in _textures.Values)
        {
            obj.Dispose();
        }
    }
            
    static VertexAttribPointerType GetPointerType(ShaderDataType type)
    {
        return type switch
        {
            ShaderDataType.Float => VertexAttribPointerType.Float,
            ShaderDataType.Uint => VertexAttribPointerType.UnsignedInt,
            ShaderDataType.Byte => VertexAttribPointerType.UnsignedByte,
            _ => throw new NotImplementedException()
        };
    }

    static BufferUsageHint GetUsageHint(BufferHint bufferHint)
    {
        return bufferHint is BufferHint.Static ? BufferUsageHint.StaticDraw : BufferUsageHint.DynamicDraw;
    }


    internal static void ThrowOnGLError()
    {
        var error = GLCheckError();
        if (error is not ErrorCode.NoError)
            throw new Exception($"OpenGL Error: {error}");
    }

    internal static ErrorCode GLCheckError()
    {
        ErrorCode error = GL.GetError();
        while (error != ErrorCode.NoError)
        {
            Console.WriteLine($"OpenGL Error: {error}");

            var ret = error;
            error = GL.GetError();
            return ret;
        }

        return ErrorCode.NoError;
    }

}
