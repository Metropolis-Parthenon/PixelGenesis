using ImGuiNET;
using PixelGenesis.Editor.Core;

namespace PixelGenesis.Editor.GUI;

internal sealed class PixelGenesisEditor(
    MenuItemGUIRenderer menuItemGUIRenderer,
    EditorWindowsGUIRenderer windowsGUIRenderer
    )
{
    public void OnGui()
    {
        // Enable Docking
        ImGui.DockSpaceOverViewport();

        menuItemGUIRenderer.OnGui();
        windowsGUIRenderer.OnGui();
    }
}
