using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using PixelGenesis._3D.Common.Components;
using PixelGenesis._3D.Common.Components.Lighting;
using PixelGenesis._3D.Renderer.DeviceApi.Abstractions;
using PixelGenesis._3D.Renderer.DrawPipeline;
using PixelGenesis.ECS;
using PixelGenesis.ECS.AssetManagement;
using PixelGenesis.ECS.Scene;
using PixelGenesis.Editor.Core;
using PixelGenesis.Editor.Services;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace PixelGenesis.Editor.BuiltIn.AssetEditors;

public class SceneEditor(
    IDeviceApi deviceApi, 
    PGScene pGScene,
    IPGWindow window,
    IEditorAssetManager assetManager) : IAssetEditor
{
    ForwardRenderer renderer = new ForwardRenderer(pGScene, deviceApi, window);
    IFrameBuffer frameBuffer;
        
    PerspectiveCameraComponent SceneCamera = new PerspectiveCameraComponent(new Transform3DComponent());

    bool IsInitialized = false;

    public bool IsDirty => false;

    public void OnGui()
    {           
        if(frameBuffer is null)
        {
            return;
        }
            
        ImGui.Image(frameBuffer.GetTexture().Id, ImGui.GetContentRegionAvail(), new Vector2(0,1), new Vector2(1,0));

    }

    public void BeforeGui()
    {
        if (!IsInitialized)
        {
            Intialize();
            return;
        }
        frameBuffer.Bind();
        GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        renderer.Update();
        renderer.Draw();
        frameBuffer.Unbind();
    }

    void Intialize()
    {
        frameBuffer = deviceApi.CreateFrameBuffer((int)window.Width, (int)window.Height);

        var light = pGScene.Create("PointLight");
        var pointLightComponent = light.AddComponent<PointLightComponent>();
        pointLightComponent.Color = new Vector3(1, 1, 1);
        pointLightComponent.Intensity = 1f;
        pointLightComponent.Transform.Position = new Vector3(0, 0, -1);

        SceneCamera.GetTransform().Position = new Vector3(0, 0, 5);

        frameBuffer.Bind();
        renderer.Initialize();
        frameBuffer.Unbind();

        renderer.CameraComponent = SceneCamera;

        

        IsInitialized = true;
    }

    public void OnSave()
    {
        if(IsDirty)
        {
            assetManager.SaveAsset(pGScene);
        }        
    }

    public void OnClose() { }
}

public class SceneEditorFactory(IDeviceApi deviceApi, IEditorAssetManager assetManager, IPGWindow window) : IAssetEditorFactory
{
    public IAssetEditor CreateAssetEditor(IAsset asset)
    {
        return new SceneEditor(deviceApi, Unsafe.As<PGScene>(asset), window, assetManager);
    }
}

