using PixelGenesis.Editor.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelGenesis.Editor.BuiltIn.MenuAction;

internal class CreateNewProject : IEditorMenuAction
{    
    public string Path => "File/New Project";

    public string Shortcut => "";

    public void OnAction()
    {
        
    }
}
