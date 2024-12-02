namespace PixelGenesis.Editor.Core;

public interface IEditorWindow
{
    public string Name { get; }    
    void OnGui();
    void OnBeforeGui() { }
}
