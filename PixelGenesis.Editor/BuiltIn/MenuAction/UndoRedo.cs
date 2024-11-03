using PixelGenesis.Editor.Core;
using PixelGenesis.Editor.Services;

namespace PixelGenesis.Editor.BuiltIn.MenuAction;

internal class Undo(IEditionCommandDispatcher commandDispatcher) : IEditorMenuAction
{
    public string Path => "Edit/Undo";

    public string Shortcut => "Ctrl+Z";

    public void OnAction()
    {
        commandDispatcher.Undo();
    }
}

internal class Redo(IEditionCommandDispatcher commandDispatcher) : IEditorMenuAction
{
    public string Path => "Edit/Redo";

    public string Shortcut => "Ctrl+Y";

    public void OnAction()
    {
        commandDispatcher.Redo();
    }
}