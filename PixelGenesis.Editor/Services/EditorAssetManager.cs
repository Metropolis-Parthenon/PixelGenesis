using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using PixelGenesis.ECS.AssetManagement;
using PixelGenesis.ECS.DataStructures;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PixelGenesis.Editor.Services;

internal class EditorAssetManager(SolutionService solutionService, IServiceProvider provider) : IEditorAssetManager
{
    const string ReferenceFileName = "AssetReferences.pgproj";

    Dictionary<Guid, string> AssetsRelativePath = new();
    Dictionary<string, Guid> AssetsIdByRelativePath = new();

    LRUCache<Guid, IAsset> InMemoryAssets = new(100);
    bool isIntilialized = false;

    public T LoadAsset<T>(Guid id) where T : class, IAsset
    {
        return Unsafe.As<T>(LoadAsset(id));
    }

    public IAsset LoadAssetFromFile(string relativePath)
    {
        InitializeIfNotInitialized();
        if (AssetsIdByRelativePath.TryGetValue(relativePath, out var id))
        {
            return LoadAsset(id);
        }

        throw new InvalidOperationException("Asset not found");
    }

    public IAsset LoadAsset(Guid id)
    {
        var asset = InMemoryAssets.GetOrAdd(id, (_) =>
        {
            InitializeIfNotInitialized();
            var assetsPath = GetAssetPath();
            if (assetsPath is null)
            {
                throw new InvalidOperationException("Project not opened");
            }

            var assetRelativePath = AssetsRelativePath[id];
            var assetAbsolutePath = Path.Combine(assetsPath, assetRelativePath);
            var extension = Path.GetExtension(assetRelativePath);

            var factory = provider.GetRequiredKeyedService<IReadAssetFactory>(extension);

            using var fileStream = File.OpenRead(assetAbsolutePath);
            return factory.ReadAsset(id, this, fileStream);
        });

        return asset;
    }

    public void SaveOrCreateInPath(IAsset asset, string path)
    {
        if (AssetsRelativePath.TryGetValue(asset.Id, out var existingPath))
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
        var assetsPath = GetProjectPath();
        if (assetsPath is null)
        {
            return;
        }

        var absolutePath = Path.Combine(assetsPath, relativePath);

        var containingFolder = Path.GetDirectoryName(absolutePath);

        if (!Directory.Exists(containingFolder))
        {
            Directory.CreateDirectory(containingFolder ?? "");
        }

        using var fileStream = File.OpenWrite(absolutePath);

        asset.WriteToStream(this, fileStream);

        ref var savedPath = ref CollectionsMarshal.GetValueRefOrAddDefault(AssetsRelativePath, asset.Id, out var exists);
        if (!exists)
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

    string? GetProjectPath()
    {
        return Path.GetDirectoryName(solutionService?.EntryProject?.FilePath);
    }

    public string? GetAssetPath()
    {
        
        var projectPath = GetProjectPath();
        if (projectPath is null)
        {
            return null;
        }    

        return Path.Combine(projectPath, "Assets");
    }

    void InitializeIfNotInitialized()
    {
        if (isIntilialized)
        {
            return;
        }

        var projectPath = GetProjectPath();
        if(projectPath is null)
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

        while (reader.Peek() > 0)
        {
            var id = Guid.Parse(reader.ReadLine() ?? throw new InvalidDataException());
            var relativePath = reader.ReadLine() ?? throw new InvalidDataException();

            AssetsRelativePath.Add(id, relativePath);
            AssetsIdByRelativePath.Add(relativePath, id);
        }

        isIntilialized = true;
    }

    void SaveAssetReferences()
    {
        InitializeIfNotInitialized();

        var projectPath = GetProjectPath();
        if (projectPath is null)
        {
            return;
        }

        using var writer = new StringWriter();
        foreach (var (id, path) in AssetsRelativePath)
        {
            writer.WriteLine(id);
            writer.WriteLine(path);
        }

        File.WriteAllText(Path.Combine(projectPath, ReferenceFileName), writer.ToString());
    }

}



public interface IEditorAssetManager : IAssetManager
{
    IAsset LoadAssetFromFile(string relativePath);    
    string? GetAssetPath();
}

