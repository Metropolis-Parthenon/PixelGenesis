using PixelGenesis._3D.Renderer.DeviceApi.Abstractions;
using PixelGenesis.ECS.AssetManagement;
using System.Diagnostics;
using System.Formats.Tar;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PixelGenesis._3D.Common;

public sealed class CompiledShader
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Reference { get; }
    public CompiledShaderDTO Layout { get; }
    public ReadOnlyMemory<byte> Vertex { get; }
    public ReadOnlyMemory<byte> Fragment { get;}
    public ReadOnlyMemory<byte> Tessellation { get; }
    public ReadOnlyMemory<byte> Geometry { get; set; }

    internal CompiledShader(
        CompiledShaderDTO layout, 
        ReadOnlyMemory<byte> vertex,
        ReadOnlyMemory<byte> fragment,
        ReadOnlyMemory<byte> tessellation,
        ReadOnlyMemory<byte> geometry)
    {
        Layout = layout;
        Vertex = vertex;
        Fragment = fragment;
        Tessellation = tessellation;
        Geometry = geometry;        
    }
}

public sealed class PGGLSLShaderSource : IAsset
{    
    static ISerializer _serializer 
        = new SerializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

    

    public ShaderSourceDTO Layout { get; set; }

    public Guid Id { get; }
    public string Name { get; }
    private PGGLSLShaderSource(Guid id, ShaderSourceDTO shaderDTO, string? name = default)
    {
        Id = id;
        Name = name ?? $"{id}.pgshader";
        Layout = shaderDTO;
    }

    Dictionary<string, CompiledShader> CompiledShaders = new Dictionary<string, CompiledShader>();

    public record CompilationResult(CompiledShader? Shader, string? Error);
    
    public CompilationResult CompiledShader(
        ReadOnlySpan<KeyValuePair<string, string>> defines,
        string vertexShaderPreCode,
        string fragmentShaderPreCode,
        string geometryShaderPreCode,
        string tessallationShaderPreCode
        )
    {
        var layout = new CompiledShaderDTO
        {
            Blocks = Layout.Blocks,
            Textures = Layout.Textures
        };

        StringBuilder injectedSourceCode = new StringBuilder();

        var span = defines;

        injectedSourceCode.AppendLine("#version 450");

        foreach (var define in span)
        {
            injectedSourceCode.AppendLine($"#define {define.Key} {define.Value}");
        }

        string? vertexSourceCode = null;
        string? fragmentSourceCode = null;
        string? tessellationSourceCode = null;
        string? geometrySourceCode = null;

        if(Layout.Vertex.Source is not null)
        {
            vertexSourceCode = 
                $"""
                {injectedSourceCode}

                {vertexShaderPreCode}

                {Layout.Vertex.Source}
                """;
        }

        if(Layout.Fragment.Source is not null)
        {
            fragmentSourceCode =
                $"""
                {injectedSourceCode}

                {fragmentShaderPreCode}

                {Layout.Fragment.Source}
                """;
        }

        if(Layout.Tessellation.Source is not null)
        {
            tessellationSourceCode =
                $"""
                {injectedSourceCode}

                {tessallationShaderPreCode}

                {Layout.Tessellation.Source}
                """;
        }

        if (Layout.Geometry.Source is not null) 
        {
            geometrySourceCode =
                $"""
                {injectedSourceCode}

                {tessallationShaderPreCode}

                {Layout.Geometry.Source}
                """;
        }

        var hash = CreateMD5($"{vertexSourceCode}\n{fragmentSourceCode}\n{tessellationSourceCode}\n{geometrySourceCode}");

        ref var cachedShader = ref CollectionsMarshal.GetValueRefOrAddDefault(CompiledShaders, hash, out bool existed);

        if(existed)
        {
            return new CompilationResult(cachedShader, null);
        }

        try
        {
            var vertex = vertexSourceCode is not null ? ShadersHelper.CompileGLSLSourceToSpirvBytecode(vertexSourceCode, "vert") : ReadOnlyMemory<byte>.Empty;
            var fragment = fragmentSourceCode is not null ? ShadersHelper.CompileGLSLSourceToSpirvBytecode(fragmentSourceCode, "frag") : ReadOnlyMemory<byte>.Empty;
            var tessellation = tessellationSourceCode is not null ? ShadersHelper.CompileGLSLSourceToSpirvBytecode(tessellationSourceCode, "tess") : ReadOnlyMemory<byte>.Empty;
            var geometry = geometrySourceCode is not null ? ShadersHelper.CompileGLSLSourceToSpirvBytecode(geometrySourceCode, "geom") : ReadOnlyMemory<byte>.Empty;

            return new CompilationResult(cachedShader = new CompiledShader(layout, vertex, fragment, tessellation, geometry), null);
        }
        catch(Exception e)
        {
            return new CompilationResult(null, e.Message);
        }        
    }

    public static string CreateMD5(string input)
    {
        // Use input string to calculate MD5 hash
        using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
        {
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            return Convert.ToHexString(hashBytes);     
        }
    }

    public void WriteToStream(IAssetManager assetManager, Stream stream)
    {
        using var tw = new StreamWriter(stream);
        _serializer.Serialize(tw, Layout);
    }

    public class Factory : IReadAssetFactory
    {
        static IDeserializer _deserializer
            = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        public IAsset ReadAsset(Guid id, IAssetManager assetManager, Stream stream)
        {
            ShaderSourceDTO dto;
            dto = _deserializer.Deserialize<ShaderSourceDTO>(new StreamReader(stream));                   
            return new PGGLSLShaderSource(id, dto);
        }
    }
}

public class ShaderSourceDTO
{
    public List<ParameterBlockDTO> Blocks { get; set; } = new List<ParameterBlockDTO>();

    public List<TextureSamplerDTO> Textures { get; set; } = new List<TextureSamplerDTO>();

    public IndividualShaderSourceDTO Vertex { get; set; } = new IndividualShaderSourceDTO();

    public IndividualShaderSourceDTO Fragment { get; set; } = new IndividualShaderSourceDTO();

    public IndividualShaderSourceDTO Tessellation {  get; set; } = new IndividualShaderSourceDTO();

    public IndividualShaderSourceDTO Geometry { get; set; } = new IndividualShaderSourceDTO();
}

public class IndividualShaderSourceDTO
{
    public List<string> Includes { get; set; } = new List<string>();

    public string? Source { get; set; }
}

public class CompiledShaderDTO
{
    public List<ParameterBlockDTO> Blocks { get; set; } = new List<ParameterBlockDTO>();
    public List<TextureSamplerDTO> Textures { get; set; } = new List<TextureSamplerDTO>();
}

public class TextureSamplerDTO
{
    public string Name { get; set; }    
}

public class ParameterBlockDTO
{
    public string Name { get; set; }
    public List<ParameterDTO> Parameters { get; set; } = new List<ParameterDTO>();
}

public class ParameterDTO
{
    public string Name { get; set; }
    public Type Type { get; set; }
    public ParameterValueRangeDTO? Range { get; set; }
}

public class ParameterValueRangeDTO
{
    public float Min { get; set; }
    public float Max { get; set; }
}

public enum Type
{
    Float,
    Float2,
    Float3,
    Float4,
    Mat4,
    Color4
}