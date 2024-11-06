using PixelGenesis.ECS;
using System.Numerics;

namespace PixelGenesis._3D.Common;

public class Material : IWritableAsset, IReadableAsset
{
    public string Reference { get; set; }

    public CompiledShader Shader { get; set; }

    Dictionary<int, object> Parameters = new Dictionary<int, object>();
    Dictionary<int, Texture> Textures = new Dictionary<int, Texture>();


    public void SetParameter(int location, Matrix4x4 value)
    {
        Parameters[location] = value;
    }

    public void SetParameter(int location, Vector3 value)
    {
        Parameters[location] = value;
    }

    public void SetParameter(int location, Vector4 value)
    {
        Parameters[location] = value;
    }

    public void SetParameter(int location, Vector2 value)
    {
        Parameters[location] = value;
    }

    public void SetParameter(int location, float value)
    {
        Parameters[location] = value;
    }

    public void SetTexture(int location, Texture texture)
    {
        Textures[location] = texture;
    }

    private Material(string reference, CompiledShader shader)
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
            var shader = AssetReader.ReadAsset<CompiledShader, CompiledShader.Factory>(shaderReference);

            return new Material(reference, shader);
        }
    }

}
