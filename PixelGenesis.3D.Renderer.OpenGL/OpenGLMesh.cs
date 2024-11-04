using CommunityToolkit.HighPerformance;
using OpenTK.Graphics.OpenGL4;
using PixelGenesis._3D.Common;
using System.Numerics;

namespace PixelGenesis._3D.Renderer.OpenGL;

internal class OpenGLMesh(IMesh mesh) : IOpenGLObject
{
    int VertexBufferHandle;
    int IndexBufferHandle;
    int VertexArrayObject;

    ReadOnlyMemory<float> VertexBuffer => mesh.Vertices.Cast<Vector3, float>();
    ReadOnlyMemory<uint> Indices => mesh.Triangles;

    public void Create()
    {
        // vertex buffer
        GL.GenBuffers(1, out VertexBufferHandle);
        GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferHandle);

#warning "We should avoid using ToArray() here, it's not efficient"
        GL.BufferData(BufferTarget.ArrayBuffer, VertexBuffer.Length * sizeof(float), VertexBuffer.ToArray(), BufferUsageHint.StaticDraw);
        // declare vertex attributes, in this case we only have position
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 3, 0);


        //index buffer
        IndexBufferHandle = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBufferHandle);
#warning "We should avoid using ToArray() here, it's not efficient"
        GL.BufferData(BufferTarget.ElementArrayBuffer, Indices.Length * sizeof(uint), Indices.ToArray(), BufferUsageHint.StaticDraw);
    }

    public void Bind()
    {
        throw new NotImplementedException();
    }

}
