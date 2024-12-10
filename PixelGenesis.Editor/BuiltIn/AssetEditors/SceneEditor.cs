using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;
using PixelGenesis._3D.Common.Components;
using PixelGenesis._3D.Common.Components.Lighting;
using PixelGenesis._3D.Renderer.DeviceApi.Abstractions;
using PixelGenesis._3D.Renderer.DrawPipeline;
using PixelGenesis.ECS;
using PixelGenesis.ECS.AssetManagement;
using PixelGenesis.ECS.Scene;
using PixelGenesis.ECS.Systems;
using PixelGenesis.Editor.Core;
using PixelGenesis.Editor.Services;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace PixelGenesis.Editor.BuiltIn.AssetEditors;

public class SceneEditor(
    IDeviceApi deviceApi, 
    PGScene pGScene,   
    IPGWindow window,
    IWindowInputs inputs,
    ITime time,
    IEditorAssetManager assetManager) : IAssetEditor
{
    ForwardRenderer renderer;
        
    PerspectiveCameraComponent SceneCamera = new PerspectiveCameraComponent(new Transform3DComponent());
        
    bool IsInitialized = false;

    public bool IsDirty => false;

    FrameBufferGuiWindow guiWindow;

    public void OnGui()
    {
        if (!IsInitialized)
        {            
            Intialize();
            return;
        }

        UpdateCamera();

        guiWindow.OnGui();
    }

    void UpdateCamera()
    {
        if(!ImGui.IsWindowFocused())
        {
            return;
        }

        float speed = 2f;
        var cameraTransform = SceneCamera.GetTransform();
        if (inputs.IsKeyboardKeyDown(PGInputKey.W))
        {
            cameraTransform.Position += cameraTransform.Forward * speed * time.DeltaTime;
        }

        if (inputs.IsKeyboardKeyDown(PGInputKey.S))
        {
            cameraTransform.Position += cameraTransform.Backward * speed * time.DeltaTime;
        }

        if (inputs.IsKeyboardKeyDown(PGInputKey.A))
        {
            cameraTransform.Position += cameraTransform.Left * speed * time.DeltaTime;
        }

        if (inputs.IsKeyboardKeyDown(PGInputKey.D))
        {
            cameraTransform.Position += cameraTransform.Right * speed * time.DeltaTime;
        }

        if (inputs.IsKeyboardKeyDown(PGInputKey.LeftShift))
        {
            cameraTransform.Position += cameraTransform.Up * speed * time.DeltaTime;
        }

        if (inputs.IsKeyboardKeyDown(PGInputKey.LeftControl))
        {
            cameraTransform.Position += cameraTransform.Down * speed * time.DeltaTime;
        }
        if (inputs.IsKeyboardKeyDown(PGInputKey.Left))
        {
            cameraTransform.Rotate(new Vector3(0,0,1) * speed * time.DeltaTime);
        }

        if (inputs.IsKeyboardKeyDown(PGInputKey.Right))
        {
            cameraTransform.Rotate(-new Vector3(0, 0, 1) * speed * time.DeltaTime);            
        }
    }

    public void BeforeGui()
    {
        
    }

    void Intialize()
    {
        guiWindow = new FrameBufferGuiWindow(deviceApi, window, () =>
        {
            renderer.Update();
            renderer.Draw(guiWindow.FrameBufferId);
        });
        renderer = new ForwardRenderer(pGScene, deviceApi, guiWindow);
        
        var light = pGScene.Create("PointLight");
        var pointLightComponent = light.AddComponent<PointLightComponent>();
        pointLightComponent.Color = new Vector3(1, 1, 1);
        pointLightComponent.Intensity = 1f;
        pointLightComponent.Transform.Position = new Vector3(0, 0, -1);

        SceneCamera.GetTransform().Position = new Vector3(0, 0, 5);

        renderer.Initialize();
        
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

public class SceneEditorFactory(IDeviceApi deviceApi, IPGWindow window, IWindowInputs inputs, ITime time, IEditorAssetManager assetManager) : IAssetEditorFactory
{
    public IAssetEditor CreateAssetEditor(IAsset asset)
    {
        return new SceneEditor(deviceApi, Unsafe.As<PGScene>(asset), window, inputs, time, assetManager);
    }
}

