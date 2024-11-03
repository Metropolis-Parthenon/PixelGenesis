using ImGuiNET;
using PixelGenesis.Editor.Core;
using PixelGenesis.Editor.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelGenesis.Editor.BuiltIn.EditorWindows;

internal class FileEditorWindow(ImageLoader imageLoader) : IEditorWindow
{
    public string Name => "EditorWindow";

    public void OnGui()
    {
        
    }
}
