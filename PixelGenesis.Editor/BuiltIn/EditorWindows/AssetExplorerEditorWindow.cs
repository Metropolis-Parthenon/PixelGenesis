using ImGuiNET;
using PixelGenesis.Editor.Core;
using PixelGenesis.Editor.Services;


namespace PixelGenesis.Editor.BuiltIn.EditorWindows;

internal class AssetExplorerEditorWindow : IEditorWindow
{
    public const string AssetFolderName = "Assets"; 

    public string Name => "Asset Explorer";
    
    const int PathMaxLength = 64;
    
    SolutionService projectService;
    ICommandDispatcher dispatcher;

    AssetBrowser? entryAssetBrowser;
    AssetBrowser? editorAssetBrowser;

    public AssetExplorerEditorWindow(SolutionService projectService, ICommandDispatcher commandDispatcher)
    {
        this.projectService = projectService;
        commandDispatcher.GetCommands<SolutionOpened>().Subscribe(OnSolutionOpened);
        dispatcher = commandDispatcher;
    }

    void OnSolutionOpened(SolutionOpened e)
    {
        if(!projectService.HasEditorProject)
        {
            entryAssetBrowser = default;
            editorAssetBrowser = default;
            return;
        }

        var assetPath = Path.Combine(Path.GetDirectoryName(projectService.EntryProject.FilePath), AssetFolderName);
        entryAssetBrowser = new AssetBrowser(assetPath, OnEntryFileSelected, OnEntryFileOpen);

        if(projectService.HasEditorProject)
        {
            assetPath = Path.Combine(Path.GetDirectoryName(projectService.EditorProject.FilePath), AssetFolderName);
            editorAssetBrowser = new AssetBrowser(assetPath, OnEditorFileSelected, OnEditorFileOpen);
        }

    }

    private void OnEditorFileOpen(string obj)
    {
        dispatcher.Dispatch(new AssetFileOpen(ProjectType.Editor, obj));        
    }

    private void OnEditorFileSelected(string obj)
    {
        dispatcher.Dispatch(new AssetFileSelected(ProjectType.Editor, obj));        
    }

    private void OnEntryFileOpen(string obj)
    {
        dispatcher.Dispatch(new AssetFileOpen(ProjectType.Entry, obj));        
    }

    private void OnEntryFileSelected(string obj)
    {
        dispatcher.Dispatch(new AssetFileSelected(ProjectType.Entry, obj));        
    }

    public void OnGui()
    {
        if(entryAssetBrowser is null)
        {
            ImGui.Text("No project open");
            return;
        }

        if(editorAssetBrowser is not null)
        {
            ImGui.BeginTabBar("Asset Explorer");

            if (ImGui.BeginTabItem("Project Assets"))
            {
                entryAssetBrowser.OnGui();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Editor Assets"))
            {
                editorAssetBrowser.OnGui();
                ImGui.EndTabItem();
            }            

            ImGui.EndTabBar();
            return;
        }

        entryAssetBrowser.OnGui();
    }
}

internal class AssetBrowser(
    string rootAbsolutePath,
    Action<string> onFileSelected, 
    Action<string> onFileOpen)
{
    string relativePath = "";
    string? selectedFile;

    public void OnGui()
    {
        if (ImGui.Button("New"))
        {
            ImGui.BeginMenu("Folder");
            ImGui.EndMenu();
        }
        ImGui.SameLine();
        if (ImGui.Button("<-"))
        {
            if (relativePath.Length > 0)
            {
                relativePath = Path.GetDirectoryName(relativePath) ?? "";
            }
        }
        ImGui.SameLine();
        ImGui.Text($"/{relativePath}");


        ImGui.BeginChild(1);
        OnFileBrowser(rootAbsolutePath, ref relativePath);
        ImGui.EndChild();
    }


    void OnFileBrowser(string absoluteRootPath, ref string relativePath)
    {
        var absolutePath = Path.Combine(absoluteRootPath, relativePath);

        var direcories = Directory.GetDirectories(absolutePath);

        foreach (var directory in direcories)
        {
            var directoryName = Path.GetFileName(directory);
            if(ImGui.Selectable(directoryName, directory == selectedFile))
            {
                selectedFile = directory;
                onFileSelected(directory);
            }
            if(ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            {
                relativePath = Path.Combine(relativePath, directoryName);
            }
        }

        var files = Directory.GetFiles(absolutePath);
        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);
            if (ImGui.Selectable(fileName, file == selectedFile))
            {
                selectedFile = file;
                onFileSelected(file);
            }
            if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            {
                onFileOpen(file);
            }
        }

    }
}

public enum ProjectType
{
    Entry,
    Editor
}

public record AssetFileOpen(ProjectType ProjectType, string path);
public record AssetFileSelected(ProjectType ProjectType, string path);