using OpenTK.Graphics.OpenGL4;

namespace PixelGenesis.Lab.OpenGLAbstractions;

internal class VertexArrayObject
{
    int _rendererID;

    public VertexArrayObject()
    {
        _rendererID = GL.GenVertexArray();
        GL.BindVertexArray(_rendererID);
    }

    public void AddBuffer(VertexBuffer vb, VertexBufferLayout layout)
    {
        Bind();
        vb.Bind();

        var elements = layout.Elements;

        int offset = 0;

        for (int i = 0; i < elements.Length; i++)
        {
            var element = elements[i];
            GL.EnableVertexAttribArray((uint)i);
            GL.VertexAttribPointer(i, element.Count, element.Type, element.Normalized, layout.Stride, offset);

            offset += element.Count * element.Size;
        }
    }
    public void Bind()
    {
        GL.BindVertexArray(_rendererID);
    }

    public void Unbind()
    {
        GL.BindVertexArray(0);
    }

    public void Dispose()
    {
        GL.DeleteVertexArray(_rendererID);
    }
}
