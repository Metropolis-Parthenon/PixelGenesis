using PixelGenesis.ECS;

namespace PixelGenesis._3D.Common;

public class Material : IWritableAsset, IReadableAsset
{
    public string Reference { get; }

    public Shader Shader { get; set; }

    private Material(string reference, Shader shader)
    {
        Reference = reference;
        Shader = shader;
    }

    public void WriteToStream(Stream stream)
    {
        using var textWriter = new StreamWriter(stream);
        textWriter.WriteLine(Shader.Reference);
    }

    public class Factory : IReadableAssetFactory<Material>
    {
        public Material ReadAsset(string reference, Stream stream)
        {
            using var textReader = new StreamReader(stream);

            var shaderReference = textReader.ReadLine() ?? throw new InvalidOperationException();
            var shader = AssetReader.ReadAsset<Shader, Shader.Factory>(shaderReference);

            return new Material(reference, shader);
        }
    }

}
