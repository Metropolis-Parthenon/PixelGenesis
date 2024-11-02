using ImGuiNET;
using PixelGenesis.Editor.Core;

namespace PixelGenesis.Editor.GUI;

public sealed class PixelGenesisEditor
{
    List<IEditorWindow> Windows = new List<IEditorWindow>();
    List<IEditorMenuAction> EditorMenuActions = new List<IEditorMenuAction>();
        
    public void OnGui()
    {
        OnMenuBar();
    }

    void OnMenuBar()
    {
        ImGui.BeginMainMenuBar();

        if(ImGui.BeginMenu("File"))
        {
            ImGui.MenuItem("Open..", "Ctrl+O");
            ImGui.MenuItem("Save", "Ctrl+S");
            ImGui.MenuItem("Close", "Ctrl+W");
        }
        ImGui.EndMenu();

        ImGui.EndMainMenuBar();
    }
}
