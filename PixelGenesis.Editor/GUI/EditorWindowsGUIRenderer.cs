using ImGuiNET;
using PixelGenesis.Editor.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelGenesis.Editor.GUI;

internal class EditorWindowsGUIRenderer(IEnumerable<IEditorWindow> editorWindows)
{
    public void OnGui()
    {
        foreach(var editorWindow in editorWindows)
        {
            ImGui.Begin(editorWindow.Name);
                editorWindow.OnGui();
            ImGui.End();
        }
    }
}
