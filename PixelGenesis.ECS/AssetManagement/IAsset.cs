namespace PixelGenesis.ECS.AssetManagement;

public interface IAsset
{
    public Guid Id { get; }
    public string Name { get; }
    public void WriteToStream(IAssetManager assetManager, Stream stream);
}

[AttributeUsage(AttributeTargets.Class)]
public class ReadableAssetAttribute<T, F> : Attribute where T : IAsset where F : IReadAssetFactory;

public interface IReadAssetFactory
{
    public IAsset ReadAsset(Guid id, IAssetManager assetManager, Stream stream);
}