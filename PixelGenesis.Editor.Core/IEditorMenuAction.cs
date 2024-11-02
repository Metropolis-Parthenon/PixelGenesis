namespace PixelGenesis.Editor.Core;

public interface IEditorMenuAction
{
    string Path { get; }
    string Shortcut { get; }
    public void OnAction();
}
