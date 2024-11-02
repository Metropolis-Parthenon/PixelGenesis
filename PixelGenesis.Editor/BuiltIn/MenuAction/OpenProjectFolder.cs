using PixelGenesis.Editor.Core;
using PixelGenesis.Editor.Services;

namespace PixelGenesis.Editor.BuiltIn.MenuAction;

internal sealed class OpenProjectFolder(SolutionService projectService) : IEditorMenuAction
{    
    public string Path => "File/Open Project";

    public string Shortcut => "";

    public void OnAction()
    {
        var result = NativeFileDialogSharp.Dialog.FileOpen("sln");

        if(result.IsOk)
        {
            projectService.OpenSolution(result.Path);
        }
    }
}
