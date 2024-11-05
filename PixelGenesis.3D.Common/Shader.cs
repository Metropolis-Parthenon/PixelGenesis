using PixelGenesis.ECS;
using System.IO;

namespace PixelGenesis._3D.Common;

[ReadableAsset<GLSLShaderFile, Factory>]
public sealed class GLSLShaderFile : IReadableAsset, IWritableAsset
{
    public string Reference { get; }
    public ReadOnlyMemory<byte> SpirvBytecode { get; }
    
    private GLSLShaderFile(string reference, ReadOnlyMemory<byte> spirvBytecode)
    {
        Reference = reference;
        SpirvBytecode = spirvBytecode;
    }
    
    public void WriteToStream(Stream stream)
    {
        using var bw = new BinaryWriter(stream);
        bw.Write(SpirvBytecode.Span);
    }

    public class Factory : IReadableAssetFactory<GLSLShaderFile>
    {
        public GLSLShaderFile ReadAsset(string reference, Stream stream)
        {
            MemoryStream memoryStream;

            if(stream is MemoryStream ms)
            {
                memoryStream = ms;
            }
            else
            {
                memoryStream = new MemoryStream();
                stream.CopyTo(memoryStream);
            }

            var memory = memoryStream.GetBuffer().AsMemory().Slice(0, (int)memoryStream.Length);
            return new GLSLShaderFile(reference, memory);
        }
    }
}

public sealed class Shader : IWritableAsset, IReadableAsset
{
    public string Reference { get; }
    public GLSLShaderFile VertexShade { get; }
    public GLSLShaderFile FragmentShader { get; }

    private Shader(GLSLShaderFile vertexShader, GLSLShaderFile fragmentShader, string reference)
    {
        VertexShade = vertexShader;
        FragmentShader = fragmentShader;
        Reference = reference;
    }

    public void WriteToStream(Stream stream)
    {
        using var textWriter = new StreamWriter(stream);
        textWriter.WriteLine(VertexShade.Reference);
        textWriter.WriteLine(FragmentShader.Reference);
    }

    public class Factory : IReadableAssetFactory<Shader>
    {
        public Shader ReadAsset(string reference, Stream stream)
        {
            using var textReader = new StreamReader(stream);

            var vertexShaderReference = textReader.ReadLine() ?? throw new InvalidOperationException();
            var fragmentShaderReference = textReader.ReadLine() ?? throw new InvalidOperationException();

            var vertexShader = AssetReader.ReadAsset<GLSLShaderFile, GLSLShaderFile.Factory>(vertexShaderReference);
            var fragmentShader = AssetReader.ReadAsset<GLSLShaderFile, GLSLShaderFile.Factory>(fragmentShaderReference);
            return new Shader(vertexShader, fragmentShader, reference);
        }
    }
}
