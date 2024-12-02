using ImGuiNET;
using Microsoft.Extensions.DependencyInjection;
using PixelGenesis.Editor.Core;
using PixelGenesis.Editor.Services;
using System;
using System.Reactive.Linq;
using System.Runtime.InteropServices;

namespace PixelGenesis.Editor.BuiltIn.EditorWindows;

internal class FileEditorWindow : IEditorWindow
{
    public string Name => "EditorWindow";
        
    List<(string Path, IAssetEditor AssetEditor)> _openedAssets = new List<(string Path, IAssetEditor AssetEditor)>();

    IServiceProvider provider;
    IEditorAssetManager assetManager;

    public FileEditorWindow(IServiceProvider provider, IEditorAssetManager assetManager, ICommandDispatcher commandDispatcher)
    {
        this.provider = provider;
        this.assetManager = assetManager;
        commandDispatcher.Commands.Where(x => x is AssetFileOpen).Cast<AssetFileOpen>().Subscribe(OnAssetFileOpen);
    }

    private void OnAssetFileOpen(AssetFileOpen open)
    {
        var extension = Path.GetExtension(open.path);

        var factory = provider.GetKeyedService<IAssetEditorFactory>(extension);
        if(factory is null)
        {
            // TODO open with external program
            return;
        }

        var relativePath = Path.GetRelativePath(assetManager.GetAssetPath(), open.path);

        var editor = factory.CreateAssetEditor(assetManager.LoadAssetFromFile(relativePath));

        _openedAssets.Add((open.path, editor));

        //if (_editors.TryGetValue(extension, out var editor))
        //{
        //    _openedAssets.Add((open.path, editor));
        //    editor.OnOpenFile(open.path);
        //}
    }

    public void OnGui()
    {
        ImGui.BeginTabBar("Editors");

        var editors = CollectionsMarshal.AsSpan(_openedAssets);
        foreach (var (path, editor) in editors)
        {
            ImGui.BeginTabItem(Path.GetFileName(path));
            editor.OnGui();
            ImGui.EndTabItem();
        }

        ImGui.EndTabBar();
    }

    public void OnBeforeGui()
    {
        var editors = CollectionsMarshal.AsSpan(_openedAssets);
        foreach (var (path, editor) in editors)
        {            
            editor.BeforeGui();         
        }
    }

}