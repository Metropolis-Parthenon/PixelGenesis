namespace PixelGenesis.ECS.AssetManagement;

public interface IAssetManager
{
    public T LoadAsset<T>(Guid id) where T : class, IAsset;

    public IAsset LoadAsset(Guid id);

    public void SaveOrCreateInPath(IAsset asset, string path);

    public void SaveAsset(IAsset asset);

    public void SaveAsset(IAsset asset, string relativePath);

    public Guid AddAssetFromFile(string relativePath);
}