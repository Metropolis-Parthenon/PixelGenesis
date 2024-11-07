using PixelGenesis._3D.Renderer.DeviceApi.Abstractions;
using PixelGenesis.ECS;
using System.Diagnostics;
using System.Formats.Tar;
using System.IO;
using System.IO.Compression;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PixelGenesis._3D.Common;

[ReadableAsset<CompiledShader, Factory>]
public sealed class CompiledShader : IReadableAsset, IWritableAsset
{
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
        ReadOnlyMemory<byte> geometry,
        string reference)
    {
        Layout = layout;
        Vertex = vertex;
        Fragment = fragment;
        Tessellation = tessellation;
        Geometry = geometry;
        Reference = reference;
    }


    public void WriteToStream(Stream stream)
    {
        //using var archive = new ZipArchive(stream, ZipArchiveMode.Create);

        //CompiledShaderDTO layout;
        //var layoutEntry = archive.CreateEntry("layout.yml");
        //using (var layoutStream = layoutEntry.Open())
        //{
        //    layout = _deserializer.Deserialize<CompiledShaderDTO>(new StreamReader(layoutStream));
        //}

        //var vertexEntry = archive.GetEntry("vertex.spv");
        //var fragmentEntry = archive.GetEntry("fragment.spv");
        //var tessellationEntry = archive.GetEntry("tessellation.spv");
        //var geometryEntry = archive.GetEntry("geometry.spv");

        //ReadOnlyMemory<byte> vertex;
        //ReadOnlyMemory<byte> fragment;
        //ReadOnlyMemory<byte> tessellation;
        //ReadOnlyMemory<byte> geometry;

        //if (vertexEntry is null)
        //{
        //    vertex = ReadOnlyMemory<byte>.Empty;
        //}
        //else
        //{
        //    using (var vertexStream = vertexEntry.Open())
        //    {
        //        var ms = new MemoryStream();
        //        vertexStream.CopyTo(ms);
        //        vertex = ms.GetBuffer().AsMemory().Slice(0, (int)ms.Length);
        //    }
        //}

        //if (fragmentEntry is null)
        //{
        //    fragment = ReadOnlyMemory<byte>.Empty;
        //}
        //else
        //{
        //    using (var fragmentStream = fragmentEntry.Open())
        //    {
        //        var ms = new MemoryStream();
        //        fragmentStream.CopyTo(ms);
        //        fragment = ms.GetBuffer().AsMemory().Slice(0, (int)ms.Length);
        //    }
        //}

        //if (tessellationEntry is null)
        //{
        //    tessellation = ReadOnlyMemory<byte>.Empty;
        //}
        //else
        //{
        //    using (var tessellationStream = tessellationEntry.Open())
        //    {
        //        var ms = new MemoryStream();
        //        tessellationStream.CopyTo(ms);
        //        tessellation = ms.GetBuffer().AsMemory().Slice(0, (int)ms.Length);
        //    }
        //}

        //if (geometryEntry is null)
        //{
        //    geometry = ReadOnlyMemory<byte>.Empty;
        //}
        //else
        //{
        //    using (var geometryStream = geometryEntry.Open())
        //    {
        //        var ms = new MemoryStream();
        //        geometryStream.CopyTo(ms);
        //        geometry = ms.GetBuffer().AsMemory().Slice(0, (int)ms.Length);
        //    }
        //}

        //return new CompiledShader(layout, vertex, fragment, tessellation, geometry, reference);
        throw new NotImplementedException();
    }


    public class Factory : IReadableAssetFactory<CompiledShader>
    {
        static IDeserializer _deserializer
            = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        public CompiledShader ReadAsset(string reference, Stream stream)
        {
            using var archive = new ZipArchive(stream, ZipArchiveMode.Create);

            CompiledShaderDTO layout;
            var layoutEntry = archive.CreateEntry("layout.yml");
            using (var layoutStream = layoutEntry.Open())
            {
                layout = _deserializer.Deserialize<CompiledShaderDTO>(new StreamReader(layoutStream));
            }

            var vertexEntry = archive.GetEntry("vertex.spv");
            var fragmentEntry = archive.GetEntry("fragment.spv");
            var tessellationEntry = archive.GetEntry("tessellation.spv");
            var geometryEntry = archive.GetEntry("geometry.spv");

            ReadOnlyMemory<byte> vertex;
            ReadOnlyMemory<byte> fragment;
            ReadOnlyMemory<byte> tessellation;
            ReadOnlyMemory<byte> geometry;

            if (vertexEntry is null)
            {
                vertex = ReadOnlyMemory<byte>.Empty;
            }
            else
            {
                using (var vertexStream = vertexEntry.Open())
                {
                    var ms = new MemoryStream();
                    vertexStream.CopyTo(ms);
                    vertex = ms.GetBuffer().AsMemory().Slice(0, (int)ms.Length);
                }
            }

            if (fragmentEntry is null)
            {
                fragment = ReadOnlyMemory<byte>.Empty;
            }
            else
            {
                using (var fragmentStream = fragmentEntry.Open())
                {
                    var ms = new MemoryStream();
                    fragmentStream.CopyTo(ms);
                    fragment = ms.GetBuffer().AsMemory().Slice(0, (int)ms.Length);
                }
            }

            if (tessellationEntry is null)
            {
                tessellation = ReadOnlyMemory<byte>.Empty;
            }
            else
            {
                using (var tessellationStream = tessellationEntry.Open())
                {
                    var ms = new MemoryStream();
                    tessellationStream.CopyTo(ms);
                    tessellation = ms.GetBuffer().AsMemory().Slice(0, (int)ms.Length);
                }
            }

            if (geometryEntry is null)
            {
                geometry = ReadOnlyMemory<byte>.Empty;
            }
            else
            {
                using (var geometryStream = geometryEntry.Open())
                {
                    var ms = new MemoryStream();
                    geometryStream.CopyTo(ms);
                    geometry = ms.GetBuffer().AsMemory().Slice(0, (int)ms.Length);
                }
            }

            return new CompiledShader(layout, vertex, fragment, tessellation, geometry, reference);
        }
    }
}


[ReadableAsset<PGGLSLShaderSource, Factory>]
public sealed class PGGLSLShaderSource : IReadableAsset, IWritableAsset
{    
    static ISerializer _serializer 
        = new SerializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

    static IDeserializer _deserializer
        = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

    public string Reference { get; }
    public ShaderSourceDTO ShaderDTO { get; set; }

    private PGGLSLShaderSource(string reference, ShaderSourceDTO shaderDTO)
    {
        Reference = reference;
        ShaderDTO = shaderDTO;
    }

    const string GLSLCPath = "C:\\VulkanSDK\\1.3.296.0\\Bin\\glslc.exe";

    public record CompilationResult(CompiledShader? Shader, string? Error);
    
    public CompilationResult CompiledShader()
    {
        var layout = new CompiledShaderDTO
        {
            Blocks = ShaderDTO.Blocks,
            Textures = ShaderDTO.Textures
        };

        try
        {
            var vertex = ShaderDTO.Vertex is not null ? ShadersHelper.CompileGLSLSourceToSpirvBytecode(ShaderDTO.Vertex, "vert") : ReadOnlyMemory<byte>.Empty;
            var fragment = ShaderDTO.Fragment is not null ? ShadersHelper.CompileGLSLSourceToSpirvBytecode(ShaderDTO.Fragment, "frag") : ReadOnlyMemory<byte>.Empty;
            var tessellation = ShaderDTO.Tessellation is not null ? ShadersHelper.CompileGLSLSourceToSpirvBytecode(ShaderDTO.Tessellation, "tess") : ReadOnlyMemory<byte>.Empty;
            var geometry = ShaderDTO.Geometry is not null ? ShadersHelper.CompileGLSLSourceToSpirvBytecode(ShaderDTO.Geometry, "geom") : ReadOnlyMemory<byte>.Empty;

            return new CompilationResult(new CompiledShader(layout, vertex, fragment, tessellation, geometry, Reference), null);
        }
        catch(Exception e)
        {
            return new CompilationResult(null, e.Message);
        }
        
    }

    static Memory<byte> CompileToSpirv(string source)
    {
        var sourcePath = Path.GetTempFileName();
        var outputPath = Path.GetTempFileName();
        File.WriteAllText(sourcePath, source);

        var startInfo = new ProcessStartInfo()
        {
            FileName = GLSLCPath,
            ArgumentList = { sourcePath, "-o", outputPath },
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

        var bytecode = File.ReadAllBytes(outputPath);

        File.Delete(sourcePath);
        File.Delete(outputPath);

        return bytecode;
    }

    public void WriteToStream(Stream stream)
    {
        using var tw = new StreamWriter(stream);
        _serializer.Serialize(tw, ShaderDTO);
    }

    public class Factory : IReadableAssetFactory<PGGLSLShaderSource>
    {
        public PGGLSLShaderSource ReadAsset(string reference, Stream stream)
        {
            ShaderSourceDTO dto;
            try
            {
                dto = _deserializer.Deserialize<ShaderSourceDTO>(new StreamReader(stream));
            }
            catch (Exception e)
            {
                return new PGGLSLShaderSource(reference, null);
            }
            
            return new PGGLSLShaderSource(reference, dto);
        }
    }
}

public class ShaderSourceDTO
{
    public List<ParameterBlockDTO> Blocks { get; set; } = new List<ParameterBlockDTO>();

    public List<TextureSamplerDTO> Textures { get; set; } = new List<TextureSamplerDTO>();

    public string? Vertex { get; set; }

    public string? Fragment { get; set; }

    public string? Tessellation {  get; set; }

    public string? Geometry { get; set; }
}

public class CompiledShaderDTO
{
    public List<ParameterBlockDTO> Blocks { get; set; } = new List<ParameterBlockDTO>();
    public List<TextureSamplerDTO> Textures { get; set; } = new List<TextureSamplerDTO>();
}

public class TextureSamplerDTO
{
    public string Name { get; set; }
    public int Binding { get; set; }
}

public class ParameterBlockDTO
{
    public int Binding { get; set; }
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