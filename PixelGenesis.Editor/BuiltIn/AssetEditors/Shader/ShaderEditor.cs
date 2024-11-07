using ImGuiNET;
using PixelGenesis._3D.Common;
using PixelGenesis._3D.Renderer.DeviceApi.Abstractions;
using PixelGenesis.Editor.Core;
using System.Numerics;

namespace PixelGenesis.Editor.BuiltIn.AssetEditors.Shader;

internal class ShaderEditor(IDeviceApi deviceApi) : IAssetEditor
{
    public string Name => "Shader Editor";

    public string FileExtension => ".pgs";

    Dictionary<string, (FileSystemWatcher Watcher, ShaderRenderer Renderer)> OpenedShaders = new();

    public void OnOpenFile(string filePath)
    {
        if(OpenedShaders.ContainsKey(filePath))
        {
            return;
        }

        var watcher = new FileSystemWatcher();

        watcher.Path = Path.GetDirectoryName(filePath);
        watcher.Filter = Path.GetFileName(filePath);

        watcher.NotifyFilter = NotifyFilters.LastAccess |
                NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;

        watcher.Changed += Watcher_Changed;

        var source = new PGGLSLShaderSource.Factory().ReadAsset(filePath, File.OpenRead(filePath));
        
        var renderer = new ShaderRenderer(source, deviceApi);
        OpenedShaders.Add(filePath, (watcher, renderer));
    }

    private void Watcher_Changed(object sender, FileSystemEventArgs e)
    {
        if (OpenedShaders.TryGetValue(e.FullPath, out var shader))
        {
            using var fileStream = File.OpenRead(e.FullPath);
            var source = new PGGLSLShaderSource.Factory().ReadAsset(e.FullPath, fileStream);
            shader.Renderer.OnSourceChanged(source);            
        }
    }
    public void OnCloseFile(string filePath)
    {
        var opened = OpenedShaders[filePath];
        opened.Watcher.Dispose();
        opened.Renderer.Dispose();
        OpenedShaders.Remove(filePath);
    }

    public void OnSaveFile(string filePath)
    {
        return;
    }

    public bool IsFileDirty(string filePath)
    {
        return false;
    }



    public void OnGui(string filePath)
    {
        if (!OpenedShaders.TryGetValue(filePath, out var opened))
        {
            ImGui.Text("Failed to open shader");
            return;
        }

        opened.Renderer.OnGui();
    }

}
