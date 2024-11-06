using OpenTK.Graphics.OpenGL4;
using SPIRVCross;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

using static SPIRVCross.SPIRV;

namespace PixelGenesis.Lab.OpenGLAbstractions;

internal class Shader : IDisposable
{
    int _rendererID = 0;

    const string GLSLCPath = "C:\\VulkanSDK\\1.3.296.0\\Bin\\glslc.exe";

    Dictionary<string, int> _uniformLocationCache = new();
    Dictionary<string, int> _uniformBlockLocationCache = new();

    public Shader(string vertexPath, string fragmentPath)
    {
        var spvVertex = CompileToSpirv(vertexPath);
        var spvFragmet = CompileToSpirv(fragmentPath);

        var verSource = CompileSpivToGLSL(spvVertex);
        var fragSource = CompileSpivToGLSL(spvFragmet);

        //var vertexSource = File.ReadAllText(vertexPath);
        //var fragmentSource = File.ReadAllText(fragmentPath);

        _rendererID = CreateShader(verSource, fragSource);
    }

    public void SetUniformBlock(string name, UniformBuffer buffer)
    {
        //var location = GetUniformBlockBinding(name);
        //if(location is -1)
        //{
        //    Console.WriteLine($"Uniform block {name} not found!");
        //    return;
        //}

        GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 0, buffer._rendererID);
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

        if (exists)
        {
            return location;
        }

        return location = GL.GetUniformLocation(_rendererID, name);
    }

    int GetUniformBlockBinding(string name)
    {
        ref var binding = ref CollectionsMarshal.GetValueRefOrAddDefault(_uniformBlockLocationCache, name, out var exists);

        if (exists)
        {
            return binding;
        }

        var index = GL.GetUniformBlockIndex(_rendererID, name);
        return 2;
    }

    static unsafe string CompileSpivToGLSL(string path)
    {
        string GetString(byte* ptr)
        {
            int length = 0;
            while (length < 4096 && ptr[length] != 0)
                length++;
            // Decode UTF-8 bytes to string.
            return Encoding.UTF8.GetString(ptr, length);
        }

        var bytecode = File.ReadAllBytes(path);

        SpvId* spirv;
        fixed (byte* ptr = bytecode)
            spirv = (SpvId*)ptr;

        uint word_count = (uint)bytecode.Length / 4;

        spvc_context context = default;
        spvc_parsed_ir ir;
        spvc_compiler compiler_glsl;
        spvc_compiler_options options;
        spvc_resources resources;
        spvc_reflected_resource* list = default;
        nuint count = default;
        spvc_error_callback error_callback = default;

        // Create context.
        spvc_context_create(&context);

        // Set debug callback.
        spvc_context_set_error_callback(context, error_callback, null);

        // Parse the SPIR-V.
        spvc_context_parse_spirv(context, spirv, word_count, &ir);

        // Hand it off to a compiler instance and give it ownership of the IR.
        spvc_context_create_compiler(context, spvc_backend.Glsl, ir, spvc_capture_mode.TakeOwnership, &compiler_glsl);

        // Do some basic reflection.
        spvc_compiler_create_shader_resources(compiler_glsl, &resources);
        spvc_resources_get_resource_list_for_type(resources, spvc_resource_type.UniformBuffer, (spvc_reflected_resource*)&list, &count);

        for (uint i = 0; i < count; i++)
        {
            Console.WriteLine("ID: {0}, BaseTypeID: {1}, TypeID: {2}, Name: {3}", list[i].id, list[i].base_type_id, list[i].type_id, GetString(list[i].name));

            uint set = spvc_compiler_get_decoration(compiler_glsl, (SpvId)list[i].id, SpvDecoration.SpvDecorationDescriptorSet);
            Console.WriteLine($"Set: {set}");

            uint binding = spvc_compiler_get_decoration(compiler_glsl, (SpvId)list[i].id, SpvDecoration.SpvDecorationBinding);
            Console.WriteLine($"Binding: {binding}");

            Console.WriteLine("=========");
        }
        Console.WriteLine("\n \n");

        // Modify options.
        spvc_compiler_create_compiler_options(compiler_glsl, &options);
        spvc_compiler_options_set_uint(options, spvc_compiler_option.GlslVersion, 330);
        spvc_compiler_options_set_bool(options, spvc_compiler_option.GlslEs, false);
        spvc_compiler_install_compiler_options(compiler_glsl, options);

        byte* result = default;
        spvc_compiler_compile(compiler_glsl, (byte*)&result);
        Console.WriteLine("Cross-compiled source: {0}", GetString(result));

        // Frees all memory we allocated so far.
        spvc_context_destroy(context);

        return GetString(result);
    }

    static string CompileToSpirv(string path)
    {
        var outputPath = Path.Combine(Path.GetDirectoryName(path), Path.GetFileName(path) + ".spv");

        var startInfo = new ProcessStartInfo()
        {
            FileName = GLSLCPath,
            ArgumentList = { path, "-o", outputPath },
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        var process = Process.Start(startInfo);

        process.WaitForExit();
        if (process.ExitCode is not 0)
        {
            var message = process.StandardError.ReadToEnd();
            throw new InvalidOperationException("Cannot compile spirv: " + message);
        }

        return outputPath;
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
