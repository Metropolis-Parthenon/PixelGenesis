using ImGuiNET;
using PixelGenesis.Editor.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelGenesis.Editor.GUI;

internal class MenuItemGUIRenderer(IEnumerable<IEditorMenuAction> menuActions)
{
    public void OnGui()
    {
        if(ImGui.BeginMainMenuBar())
        {
            CreateMenus(string.Empty, new HashSet<string>());     
        }
        ImGui.EndMainMenuBar();
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
