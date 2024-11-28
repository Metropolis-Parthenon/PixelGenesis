using OpenTK.Graphics.OpenGL4;
using PixelGenesis._3D.Renderer.DeviceApi.Abstractions;
using SPIRVCross;
using System;
using System.Text;

using static SPIRVCross.SPIRV;

namespace PixelGenesis._3D.Renderer.DeviceApi.OpenGL;

public class GLShaderProgram : IShaderProgram
{
    int _id;
    public int Id => _id;

    OpenGLDeviceApi _api;

    public GLShaderProgram(
        ReadOnlyMemory<byte> vertex,
        ReadOnlyMemory<byte> fragment,
        ReadOnlyMemory<byte> tessellation,
        ReadOnlyMemory<byte> geometry,
        OpenGLDeviceApi api)
    {
        _api = api;

        string? vertexGLSL = null, fragmentGLSL = null, tessellationGLS = null, geometryGLSL = null;

        if (vertex.Length > 0)
        {
            vertexGLSL = CompileSpivToGLSL(vertex);
        }

        if (fragment.Length > 0)
        {
            fragmentGLSL = CompileSpivToGLSL(fragment);
        }

        if (tessellation.Length > 0)
        {
            tessellationGLS = CompileSpivToGLSL(tessellation);
        }

        if (geometry.Length > 0)
        {
            geometryGLSL = CompileSpivToGLSL(geometry);
        }

        _id = CreateShaderProgram(vertexGLSL, fragmentGLSL, tessellationGLS, geometryGLSL);
        _api._shaderPrograms.Add(_id, this);
    }

    public void SetUniformBlock(int binding, IUniformBlockBuffer buffer)
    {
        buffer.Bind();
        Bind();
        GL.BindBufferBase(BufferRangeTarget.UniformBuffer, binding, buffer.Id);
        OpenGLDeviceApi.ThrowOnGLError();
    }

    public void Bind()
    {
        GL.UseProgram(_id);
        OpenGLDeviceApi.ThrowOnGLError();
    }

    public void Unbind()
    {
        GL.UseProgram(0);
        OpenGLDeviceApi.ThrowOnGLError();
    }

    public void Dispose()
    {
        GL.DeleteProgram(_id);
        OpenGLDeviceApi.ThrowOnGLError();
        _api._shaderPrograms.Remove(_id);
    }

    static int CreateShaderProgram(string? vertex, string? fragment, string? tessellation, string? geometry)
    {
        var program = GL.CreateProgram();
        OpenGLDeviceApi.ThrowOnGLError();

        int index = 0;
        Span<int> shaders = stackalloc int[4];
        
        if(vertex is not null)
        {
            var id = CompileShader(vertex, ShaderType.VertexShader);
            GL.AttachShader(program, id);
            OpenGLDeviceApi.ThrowOnGLError();
            shaders[index++] = id;
        }

        if (fragment is not null) 
        {
            var id = CompileShader(fragment, ShaderType.FragmentShader);
            GL.AttachShader(program, id);
            OpenGLDeviceApi.ThrowOnGLError();
            shaders[index++] = id;
        }

        if (tessellation is not null)
        {
            var id = CompileShader(tessellation, ShaderType.TessControlShader);
            GL.AttachShader(program, id);
            OpenGLDeviceApi.ThrowOnGLError();
            shaders[index++] = id;
        }

        if(geometry is not null)
        {
            var id = CompileShader(geometry, ShaderType.GeometryShader);
            GL.AttachShader(program, id);
            OpenGLDeviceApi.ThrowOnGLError();
            shaders[index++] = id;
        }
                
        GL.LinkProgram(program);
        OpenGLDeviceApi.ThrowOnGLError();
        GL.ValidateProgram(program);
        OpenGLDeviceApi.ThrowOnGLError();

        for (var i = 0; i < index; i++)
        {
            GL.DeleteShader(shaders[i]);
            OpenGLDeviceApi.ThrowOnGLError();
        }
            

        return program;
    }

    static int CompileShader(string source, ShaderType type)
    {
        var id = GL.CreateShader(type);
        OpenGLDeviceApi.ThrowOnGLError();
        GL.ShaderSource(id, source);
        OpenGLDeviceApi.ThrowOnGLError();
        GL.CompileShader(id);
        OpenGLDeviceApi.ThrowOnGLError();

        GL.GetShader(id, ShaderParameter.CompileStatus, out var status);
        OpenGLDeviceApi.ThrowOnGLError();
        if (status == 0)
        {
            GL.GetShaderInfoLog(id, out var infoLog);
            OpenGLDeviceApi.ThrowOnGLError();
            Console.WriteLine($"Error compiling {type}: {infoLog}");
            return 0;
        }

        return id;
    }

    static unsafe string CompileSpivToGLSL(ReadOnlyMemory<byte> bytecode)
    {
        string GetString(byte* ptr)
        {
            int length = 0;
            while (length < 4096 && ptr[length] != 0)
                length++;
            // Decode UTF-8 bytes to string.
            return Encoding.UTF8.GetString(ptr, length);
        }

        SpvId* spirv;
        fixed (byte* ptr = bytecode.Span)
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

        //// Do some basic reflection.
        //spvc_compiler_create_shader_resources(compiler_glsl, &resources);
        //spvc_resources_get_resource_list_for_type(resources, spvc_resource_type.UniformBuffer, (spvc_reflected_resource*)&list, &count);

        //for (uint i = 0; i < count; i++)
        //{
        //    Console.WriteLine("ID: {0}, BaseTypeID: {1}, TypeID: {2}, Name: {3}", list[i].id, list[i].base_type_id, list[i].type_id, GetString(list[i].name));

        //    uint set = spvc_compiler_get_decoration(compiler_glsl, (SpvId)list[i].id, SpvDecoration.SpvDecorationDescriptorSet);
        //    Console.WriteLine($"Set: {set}");

        //    uint binding = spvc_compiler_get_decoration(compiler_glsl, (SpvId)list[i].id, SpvDecoration.SpvDecorationBinding);
        //    Console.WriteLine($"Binding: {binding}");

        //    Console.WriteLine("=========");
        //}
        //Console.WriteLine("\n \n");

        // Modify options.
        spvc_compiler_create_compiler_options(compiler_glsl, &options);
        spvc_compiler_options_set_uint(options, spvc_compiler_option.GlslVersion, 330);
        spvc_compiler_options_set_bool(options, spvc_compiler_option.GlslEs, false);
        spvc_compiler_install_compiler_options(compiler_glsl, options);

        byte* result = default;
        spvc_compiler_compile(compiler_glsl, (byte*)&result);
        // Console.WriteLine("Cross-compiled source: {0}", GetString(result));

        // Frees all memory we allocated so far.
        spvc_context_destroy(context);

        var source = GetString(result);

#warning revisit this stupid thing here
        // hack to re transpile until it gets it right
        if(source.Length < 20)
        {
            source = CompileSpivToGLSL(bytecode);
        }

        return source;
    }



}
