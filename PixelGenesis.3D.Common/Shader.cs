using PixelGenesis.ECS;

namespace PixelGenesis._3D.Common;

[ReadableAsset<GLSLShaderFile, Factory>]
public sealed class GLSLShaderFile : IReadableAsset, IWritableAsset
{
    public string Reference { get; }
    public string SourceCode { get; }
    
    private GLSLShaderFile(string reference, string sourceCode)
    {
        Reference = reference;
        SourceCode = sourceCode;
    }
    
    public void WriteToStream(Stream stream)
    {
        using var sw = new StreamWriter(stream);
        using var textWriter = new StreamWriter(stream);
        textWriter.Write(SourceCode);
    }

    public class Factory : IReadableAssetFactory<GLSLShaderFile>
    {
        public GLSLShaderFile ReadAsset(string reference, Stream stream)
        {
            using var sr = new StreamReader(stream);
            return new GLSLShaderFile(reference, sr.ReadToEnd());
        }
    }
}

public sealed class Shader : IWritableAsset, IReadableAsset
{
    public string Reference { get; }
    GLSLShaderFile VertexShade { get; }
    GLSLShaderFile FragmentShader { get; }

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
