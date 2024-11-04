using OpenTK.Graphics.OpenGL4;
using PixelGenesis._3D.Common;

namespace PixelGenesis._3D.Renderer.OpenGL;

public class OpenGLShader(Shader shader) : IOpenGLObject
{
    public bool IsCompiled { get; private set; }

    public int ProgramId { get; private set; } = 0;

    public void Create()
    {
        if(IsCompiled)
        {
            return;
        }

        ProgramId = GL.CreateProgram();
        var vs = CompileShader(shader.VertexShade.SourceCode, ShaderType.VertexShader);
        var fs = CompileShader(shader.FragmentShader.SourceCode, ShaderType.FragmentShader);

        GL.AttachShader(ProgramId, vs);
        GL.AttachShader(ProgramId, fs);
        GL.LinkProgram(ProgramId);
        GL.ValidateProgram(ProgramId);

        GL.DeleteShader(vs);
        GL.DeleteShader(fs);

        IsCompiled = true;
    }

    static int CompileShader(string source, ShaderType type)
    {
        var id = GL.CreateShader(type);
        GL.ShaderSource(id, source);
        GL.CompileShader(id);

        GL.GetShader(id, ShaderParameter.CompileStatus, out var status);
        if (status == 0)
        {
            GL.GetShaderInfoLog(id, out var infoLog);
            Console.WriteLine($"Error compiling {type}: {infoLog}");
            return 0;
        }

        return id;
    }

    public void Bind()
    {
        GL.UseProgram(ProgramId);
    }
}
