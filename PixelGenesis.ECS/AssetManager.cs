using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using static PixelGenesis.ECS.PGScene;

namespace PixelGenesis.ECS;

public class AssetManager(string projectPath)
{
    const string ReferenceFileName = "Project.pgproj";
    
    public string ProjectPath => projectPath;

    Dictionary<Guid, string> AssetsRelativePath = new();
    LRUCache<Guid, IAsset> InMemoryAssets = new(100);
    bool isIntilialized = false;

    static Dictionary<string, IReadAssetFactory> AssetFactories = new();

    static AssetManager()
    {
        AddAssetLoaderFactory(new PGSceneFactory(), ".pgscene");
    }

    public static void AddAssetLoaderFactory(IReadAssetFactory factory, string fileExtension)
    {
        AssetFactories.Add(fileExtension, factory);
    }

    public T LoadAsset<T>(Guid id) where T : class, IAsset
    {
        return Unsafe.As<T>(LoadAsset(id));
    }

    public IAsset LoadAsset(Guid id)
    {
        var asset = InMemoryAssets.GetOrAdd(id, (_) =>
        {
            InitializeIfNotInitialized();
            var assetRelativePath = AssetsRelativePath[id];
            var assetAbsolutePath = Path.Combine(projectPath, assetRelativePath);
            var extension = Path.GetExtension(assetRelativePath);
            var factory = AssetFactories[extension];
            using var fileStream = File.OpenRead(assetAbsolutePath);
            return factory.ReadAsset(id, this, fileStream);
        });

        return asset;
    }
        
    public void SaveOrCreateInPath(IAsset asset, string path)
    {
        if(AssetsRelativePath.TryGetValue(asset.Id, out var existingPath))
        {
            SaveAsset(asset, existingPath);
            return;
        }

        SaveAsset(asset, path);
    }

    public void SaveAsset(IAsset asset)
    {
        var relativePath = AssetsRelativePath[asset.Id];
        SaveAsset(asset, relativePath);
    }

    public void SaveAsset(IAsset asset, string relativePath)
    {
        var absolutePath = Path.Combine(projectPath, relativePath);

        var containingFolder = Path.GetDirectoryName(absolutePath);

        if(!Directory.Exists(containingFolder))
        {
            Directory.CreateDirectory(containingFolder ?? "");
        }

        using var fileStream = File.OpenWrite(absolutePath);

        asset.WriteToStream(this, fileStream);

        ref var savedPath = ref CollectionsMarshal.GetValueRefOrAddDefault(AssetsRelativePath, asset.Id, out var exists);
        if(!exists)
        {
            savedPath = relativePath;
            SaveAssetReferences();
        }
    }

    public Guid AddAssetFromFile(string relativePath)
    {
        var mapping = AssetsRelativePath.FirstOrDefault(x => x.Value == relativePath);
        if (mapping.Value is not null) 
        { 
            return mapping.Key;
        }

        var id = Guid.NewGuid();
        AssetsRelativePath.Add(id, relativePath);
        SaveAssetReferences();
        return id;
    }

    void InitializeIfNotInitialized()
    {
        if(isIntilialized)
        {
            return;
        }

        var path = Path.Combine(projectPath, ReferenceFileName);
        if (!File.Exists(path))
        {
            isIntilialized = true;
            return;
        }

        var reader = new StringReader(File.ReadAllText(path));

        while(reader.Peek() > 0)
        {
            AssetsRelativePath.Add(Guid.Parse(reader.ReadLine() ?? throw new InvalidDataException()), reader.ReadLine() ?? throw new InvalidDataException());
        }

        isIntilialized = true;
    }

    void SaveAssetReferences()
    {
        InitializeIfNotInitialized();

        using var writer = new StringWriter();        
        foreach(var (id, path) in AssetsRelativePath)
        {
            writer.WriteLine(id);
            writer.WriteLine(path);
        }        

        File.WriteAllText(Path.Combine(projectPath, ReferenceFileName), writer.ToString());
    }
}
