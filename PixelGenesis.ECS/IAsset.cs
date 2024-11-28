namespace PixelGenesis.ECS;

public interface IAsset
{
    public Guid Id { get; }
    public string Name { get; }
    public void WriteToStream(AssetManager assetManager, Stream stream);
}

[AttributeUsage(AttributeTargets.Class)]
public class ReadableAssetAttribute<T, F> : Attribute where T : IAsset where F : IReadAssetFactory;

public interface IReadAssetFactory
{
    public IAsset ReadAsset(Guid id, AssetManager assetManager, Stream stream);
}

//public static class AssetReader
//{
//    public static string RootPath { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), "Assets");

//    public static T ReadAsset<T, F>(string reference) where T : IReadableAsset where F : IReadableAssetFactory<T>, new()
//    {     
//        var path = Path.Combine(RootPath, reference);
//        using var stream = File.OpenRead(path);

//        var factory = new F();

//        return factory.ReadAsset(reference, stream);
//    }

//}