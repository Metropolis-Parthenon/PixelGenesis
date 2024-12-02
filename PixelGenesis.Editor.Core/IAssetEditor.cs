using PixelGenesis.ECS.AssetManagement;

namespace PixelGenesis.Editor.Core;

public interface IAssetEditor
{
    public bool IsDirty {  get; }
    public void OnGui();
    public void BeforeGui();
    public void OnSave();
    public void OnClose();
}

public interface IAssetEditorFactory
{
    IAssetEditor CreateAssetEditor(IAsset asset);
}
