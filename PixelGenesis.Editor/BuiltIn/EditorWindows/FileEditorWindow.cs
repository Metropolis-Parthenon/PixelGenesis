using ImGuiNET;
using PixelGenesis.Editor.Core;
using PixelGenesis.Editor.Services;
using System.Reactive.Linq;
using System.Runtime.InteropServices;

namespace PixelGenesis.Editor.BuiltIn.EditorWindows;

internal class FileEditorWindow : IEditorWindow
{
    public string Name => "EditorWindow";

    Dictionary<string, IAssetEditor> _editors;

    List<(string Path, IAssetEditor AssetEditor)> _openedAssets = new List<(string Path, IAssetEditor AssetEditor)>();

    public FileEditorWindow(IEnumerable<IAssetEditor> assetEditors, ICommandDispatcher commandDispatcher)
    {
        _editors = assetEditors.ToDictionary(x => x.FileExtension);
        commandDispatcher.Commands.Where(x => x is AssetFileOpen).Cast<AssetFileOpen>().Subscribe(OnAssetFileOpen);
    }

    private void OnAssetFileOpen(AssetFileOpen open)
    {
        var extension = Path.GetExtension(open.path);

        if (_editors.TryGetValue(extension, out var editor))
        {
            _openedAssets.Add((open.path, editor));
            editor.OnOpenFile(open.path);
        }
    }

    public void OnGui()
    {
        ImGui.BeginTabBar("Editors");

        var editors = CollectionsMarshal.AsSpan(_openedAssets);
        foreach (var (path, editor) in editors)
        {
            ImGui.BeginTabItem(Path.GetFileName(path));
            editor.OnGui(path);
            ImGui.EndTabItem();
        }

        ImGui.EndTabBar();
    }
}
