using PixelGenesis._3D.Common.Components;
using PixelGenesis._3D.Renderer.DeviceApi.Abstractions;
using PixelGenesis._3D.Renderer.DeviceObjects;
using PixelGenesis.ECS.Scene;
using System.Numerics;

namespace PixelGenesis._3D.Renderer.DrawPipeline;

public class ForwardRenderer
{
    IPGWindow window;
    IDeviceApi deviceApi;

    public PerspectiveCameraComponent? CameraComponent { get; set; }

    DeviceRenderObjectManager manager;

    ChangesTracker changesTracker;
    MaterialLoader materialLoader;
    MeshBatcher meshBatcher;
    Instancing instancing;

    DrawContext drawContext = new DrawContext()
    {
        EnableDepthTest = true,
    };

    // we can use this buffer to send any data we want from the renderer to the shader
    // for now we are only sending the projection * view matrix
    // later we can send more data like light positions, time passed, etc
    IUniformBlockBuffer? DetailsBuffer;

    public ForwardRenderer(PGScene scene, IDeviceApi deviceApi, IPGWindow window)
    {
        manager = new DeviceRenderObjectManager(deviceApi, scene);
        changesTracker = new ChangesTracker(scene);
        materialLoader = new MaterialLoader(scene, changesTracker, manager);
        meshBatcher = new MeshBatcher(scene, changesTracker);
        instancing = new Instancing(meshBatcher, manager);

        this.window = window;
        this.deviceApi = deviceApi;
    }

    public void Initialize()
    {
        DetailsBuffer = deviceApi.CreateUniformBlockBuffer<Matrix4x4, Vector3>(BufferHint.Dynamic);

        changesTracker.Initialize();
        materialLoader.Initialize();
        meshBatcher.Initialize();
        instancing.Initialize();
    }

    public void Update()
    {
        changesTracker.Update();
        materialLoader.Update();
        meshBatcher.Update();
        instancing.Update();

        changesTracker.AfterUpdate();
        meshBatcher.AfterUpdate();
        manager.Update();        
    }

    public void Draw()
    {
        if (CameraComponent is null)
        {
            return;
        }

        var camera = CameraComponent;

        var projection = camera.GetProjectionMatrix(window.Width / window.Height);
        var view = camera.GetViewMatrix();

        var viewProjection = view * projection;

        DetailsBuffer.SetData(viewProjection, 0);
        DetailsBuffer.SetData(camera.GetTransform().Position, 1);

       // Metrics.DrawCalls = 0;
        var instancedObjects = manager.InstanceObjects;
        for (var i = 0; i < instancedObjects.Length; ++i)
        {
            var instancedObject = instancedObjects[i];

            instancedObject.Draw(DetailsBuffer, drawContext);

           // Metrics.DrawCalls++;
        }

    }
}
