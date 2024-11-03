using ImGuiNET;
using PixelGenesis.Editor.Core;
using PixelGenesis.Editor.Services;
using System.Numerics;

namespace PixelGenesis.Editor.GUI;

internal class MenuItemGUIRenderer(IEnumerable<IEditorMenuAction> menuActions, ImageLoader imageLoader)
{
    public void OnGui()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(20f, 10f));
        if (ImGui.BeginMainMenuBar())
        {            
            ImGui.Image(imageLoader.LoadImage(Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Logo", "logo.png")), new Vector2(40, 40));
            CreateMenus(string.Empty, new HashSet<string>());       
        }
        ImGui.EndMainMenuBar();
        ImGui.PopStyleVar();
    }

    void CreateMenus(string path, HashSet<string> visitedPaths)
    {
        foreach (var action in menuActions) 
        {
            if(!action.Path.AsSpan().StartsWith(path))
            {
                continue;
            }

            var span = action.Path.AsSpan();
            var rightPart = span.Slice(path.Length);
            var nextNodeIndex = rightPart.IndexOf('/');
                        
            if (nextNodeIndex == -1)
            {
                if(ImGui.MenuItem(rightPart, action.Shortcut))
                {
                    action.OnAction();
                }
                continue;
            }
                        
            var nextName = rightPart.Slice(0, nextNodeIndex);

            string nextPath;
            if (path.Length is 0)
            {
                nextPath = $"{nextName}/";
            }
            else
            {
                nextPath = $"{path}/{nextName}";
            }

            if (!visitedPaths.Contains(nextPath)) 
            {
                visitedPaths.Add(nextPath);
                if (ImGui.BeginMenu(nextName))
                {
                    CreateMenus(nextPath, visitedPaths);
                    ImGui.EndMenu();
                }                
            }            
        }
    }

}
