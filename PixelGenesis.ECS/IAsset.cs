namespace PixelGenesis.ECS;

public interface IWritableAsset
{
    public void WriteToStream(Stream stream);    
}

public interface IReadableAsset
{
    public string Reference { get; }
}

[AttributeUsage(AttributeTargets.Class)]
public class ReadableAssetAttribute<T, F> : Attribute where T : IReadableAsset where F : IReadableAssetFactory<T>;

public interface IReadableAssetFactory<T> where T : IReadableAsset
{
    public T ReadAsset(string reference, Stream stream);
}

public static class AssetReader
{
    public static string RootPath { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), "Assets");

    public static T ReadAsset<T, F>(string reference) where T : IReadableAsset where F : IReadableAssetFactory<T>, new()
    {     
        var path = Path.Combine(RootPath, reference);
        using var stream = File.OpenRead(path);

        var factory = new F();

        return factory.ReadAsset(reference, stream);
    }

}