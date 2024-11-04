using OpenTK.Graphics.OpenGL4;
using System.Numerics;
using System.Runtime.InteropServices;

namespace PixelGenesis.Lab;

internal class Shader : IDisposable
{
    int _rendererID = 0;

    Dictionary<string, int> _uniformLocationCache = new();

    public Shader(string vertexPath, string fragmentPath)
    {
        var vertexSource = File.ReadAllText(vertexPath);
        var fragmentSource = File.ReadAllText(fragmentPath);

        _rendererID = CreateShader(vertexSource, fragmentSource);
    }

    public unsafe void SetUniformMat4f(string name, Matrix4x4 matrix)
    {
        var location = GetUniformLocation(name);

        if (location is -1)
        {
            Console.WriteLine($"Uniform {name} not found!");
            return;
        }

        GL.UniformMatrix4(location, 1, false, (float*)&matrix);
    }

    public void SetUniform1i(string name, int value)
    {
        var location = GetUniformLocation(name);

        if (location is -1)
        {
            Console.WriteLine($"Uniform {name} not found!");
            return;
        }
        GL.Uniform1(location, value);
    }

    public void SetUniform4f(string name, Vector4 value)
    {
        var location = GetUniformLocation(name);

        if (location is -1)
        {
            Console.WriteLine($"Uniform {name} not found!");
            return;
        }
        GL.Uniform4(location, value.X, value.Y, value.Z, value.W);
    }

    public void Bind()
    {
        GL.UseProgram(_rendererID);
    }

    public void Unbind()
    {
        GL.UseProgram(0);
    }

    int GetUniformLocation(string name)
    {
        ref var location = ref CollectionsMarshal.GetValueRefOrAddDefault(_uniformLocationCache, name, out var exists);

        if(exists)
        {
            return location;
        }

        return location = GL.GetUniformLocation(_rendererID, name);
    }

    static int CreateShader(string vertexShader, string fragmentShader)
    {
        var program = GL.CreateProgram();

        var vs = CompileShader(vertexShader, ShaderType.VertexShader);
        var fs = CompileShader(fragmentShader, ShaderType.FragmentShader);

        GL.AttachShader(program, vs);
        GL.AttachShader(program, fs);
        GL.LinkProgram(program);
        GL.ValidateProgram(program);

        GL.DeleteShader(vs);
        GL.DeleteShader(fs);

        return program;
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

    public void Dispose()
    {
        GL.DeleteProgram(_rendererID);
    }
}
