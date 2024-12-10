using ImGuiNET;
using Microsoft.Extensions.DependencyInjection;
using PixelGenesis.Editor.BuiltIn.EditorWindows;
using PixelGenesis.Editor.Core;
using System.Reactive.Linq;

namespace PixelGenesis.Editor.Services;

internal class FileEditorWindowService
{
    Dictionary<string, IAssetEditor> _openedAssets = new ();

    IServiceProvider provider;
    IEditorAssetManager assetManager;

    public FileEditorWindowService(IServiceProvider provider, IEditorAssetManager assetManager, ICommandDispatcher commandDispatcher)
    {
        this.provider = provider;
        this.assetManager = assetManager;
        commandDispatcher.Commands.Where(x => x is AssetFileOpen).Cast<AssetFileOpen>().Subscribe(OnAssetFileOpen);
    }

    private void OnAssetFileOpen(AssetFileOpen open)
    {
        var extension = Path.GetExtension(open.path);

        var factory = provider.GetKeyedService<IAssetEditorFactory>(extension);
        if (factory is null)
        {
            // TODO open with external program
            return;
        }

        var relativePath = Path.GetRelativePath(assetManager.GetAssetPath(), open.path);

        var editor = factory.CreateAssetEditor(assetManager.LoadAssetFromFile(relativePath));

        _openedAssets.TryAdd(open.path, editor);

        //if (_editors.TryGetValue(extension, out var editor))
        //{
        //    _openedAssets.Add((open.path, editor));
        //    editor.OnOpenFile(open.path);
        //}
    }

    public void OnGui()
    {
        foreach (var (path, editor) in _openedAssets)
        {
            ImGui.Begin(Path.GetFileName(path));
            editor.OnGui();
            ImGui.End();
        }
    }

    public void OnBeforeGui()
    {
        foreach (var (path, editor) in _openedAssets)
        {
            editor.BeforeGui();
        }
    }

}