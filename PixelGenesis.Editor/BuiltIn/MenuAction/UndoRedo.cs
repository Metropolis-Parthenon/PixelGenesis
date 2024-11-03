using PixelGenesis.Editor.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelGenesis.Editor.BuiltIn.MenuAction;

internal class Undo : IEditorMenuAction
{
    public string Path => "Edit/Undo";

    public string Shortcut => "Ctrl+Z";

    public void OnAction()
    {
    }
}

internal class Redo : IEditorMenuAction
{
    public string Path => "Edit/Redo";

    public string Shortcut => "Ctrl+Y";

    public void OnAction()
    {
    }
}