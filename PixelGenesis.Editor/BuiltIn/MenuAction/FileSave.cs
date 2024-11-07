using PixelGenesis.Editor.Core;
using PixelGenesis.Editor.Services;

namespace PixelGenesis.Editor.BuiltIn.MenuAction;

internal class FileSave(SaveService saveService) : IEditorMenuAction
{
    public string Path => saveService.IsCurrentDirty ? "File/Save*" : "File/Save";

    public string Shortcut => "Ctrl+S";

    public void OnAction()
    {
        saveService.SaveCurrent();
    }
}

internal class FileSaveAll(SaveService saveService) : IEditorMenuAction
{
    public string Path => saveService.IsAnyDirty ? "File/Save All*" : "File/Save All";

    public string Shortcut => "Ctrl+Shift+S";

    public void OnAction()
    {
        saveService.SaveAll();
    }
}