namespace PixelGenesis.Editor.Core;

public interface IAssetEditor
{
    public string Name { get; }
    public string FileExtension { get; }

    public void OnOpenFile(string filePath);
    public void OnSaveFile(string filePath);
    public void OnCloseFile(string filePath);
    public bool IsFileDirty(string filePath);

    public void OnGui(string filePath);
}
