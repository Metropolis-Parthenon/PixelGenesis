using OpenTK.Graphics.OpenGL4;
using PixelGenesis.Lab.OpenGLAbstractions;

namespace PixelGenesis.Lab;

internal class Renderer
{
    public void Draw(VertexArrayObject vao, IndexBuffer ib, Shader shader)
    {
        shader.Bind();
        vao.Bind();
        ib.Bind();

        GL.DrawElements(PrimitiveType.Triangles, ib.Count, DrawElementsType.UnsignedInt, 0);
    }

    public static void GLClearError()
    {
        while (GL.GetError() != ErrorCode.NoError) ;
    }

    public static void ThrowOnGLError()
    {
        if (GLCheckError() == false)
            throw new Exception("OpenGL Error");
    }

    public static bool GLCheckError()
    {
        ErrorCode error = GL.GetError();
        while (error != ErrorCode.NoError)
        {
            Console.WriteLine($"OpenGL Error: {error}");

            error = GL.GetError();
            return false;
        }

        return true;
    }

}
