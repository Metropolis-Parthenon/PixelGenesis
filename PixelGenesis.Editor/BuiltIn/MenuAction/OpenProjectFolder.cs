using PixelGenesis.Editor.Core;

namespace PixelGenesis.Editor.BuiltIn.MenuAction;

internal sealed class OpenProjectFolder : IEditorMenuAction
{
    readonly static string[] path = ["File", "Open Project Folder"];
    public string[] Path => path;

    public string Shortcut => "";

    public void OnAction()
    {
        
    }
}
