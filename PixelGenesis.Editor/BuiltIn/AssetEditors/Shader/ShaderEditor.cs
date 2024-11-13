using ImGuiNET;
using PixelGenesis._3D.Common;
using PixelGenesis._3D.Renderer.DeviceApi.Abstractions;
using PixelGenesis.Editor.Core;
using System.Numerics;

namespace PixelGenesis.Editor.BuiltIn.AssetEditors.Shader;

internal class ShaderEditor(IDeviceApi deviceApi, ICommandDispatcher commandDispatcher) : IAssetEditor
{
    public string Name => "Shader Editor";

    public string FileExtension => ".pgs";

    Dictionary<string, ShaderRenderer> OpenedShaders = new();

    public void OnOpenFile(string filePath)
    {
        if(OpenedShaders.ContainsKey(filePath))
        {
            return;
        }

        using var fileStream = File.OpenRead(filePath);

        var source = new PGGLSLShaderSource.Factory().ReadAsset(filePath, File.OpenRead(filePath));
        
        var renderer = new ShaderRenderer(source, deviceApi);
        OpenedShaders.Add(filePath, renderer);
    }

    public void OnCloseFile(string filePath)
    {
        var renderer = OpenedShaders[filePath];
        renderer.Dispose();
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
        if (!OpenedShaders.TryGetValue(filePath, out var renderer))
        {
            ImGui.Text("Failed to open shader");
            return;
        }

        if(ImGui.Button("Reload"))
        {
            using var fileStream = File.OpenRead(filePath);
            var source = new PGGLSLShaderSource.Factory().ReadAsset(filePath, fileStream);
            //renderer.OnSourceChanged(source);
        }

        renderer.OnGui();
    }

}
