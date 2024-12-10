using PixelGenesis._3D.Common.Components;
using PixelGenesis._3D.Renderer.DeviceApi.Abstractions;
using PixelGenesis._3D.Renderer.DeviceObjects;
using PixelGenesis.ECS.Scene;
using System.Drawing;
using System.Numerics;

namespace PixelGenesis._3D.Renderer.DrawPipeline;

public class ForwardRenderer : IDisposable
{
    IPGWindow window;
    IDeviceApi deviceApi;

    IFrameBuffer? sceneBuffer;

    IDisposable OnResizeSubscription;

    public PerspectiveCameraComponent? CameraComponent { get; set; }

    DeviceRenderObjectManager manager;

    ChangesTracker changesTracker;
    MaterialLoader materialLoader;
    MeshBatcher meshBatcher;
    Instancing instancing;

    PostProcessing postProcessing;

    DrawContext drawContext = new DrawContext()
    {
        EnableCullFace = true,
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

        postProcessing = new PostProcessing(deviceApi);

        this.window = window;
        this.deviceApi = deviceApi;

        OnResizeSubscription = window.ViewportSizeObservable.Subscribe(OnResizeViewport);
    }

    public void Initialize()
    {        
        DetailsBuffer = deviceApi.CreateUniformBlockBuffer<Matrix4x4, Vector3>(BufferHint.Dynamic);

        changesTracker.Initialize();
        materialLoader.Initialize();
        meshBatcher.Initialize();
        instancing.Initialize(); 
        postProcessing.Initialize();
    }

    void OnResizeViewport(Size size)
    {
        if(size.Width <=  0 || size.Height <= 0)
        {
            return;
        }

        if(sceneBuffer is null)
        {
            sceneBuffer = deviceApi.CreateFrameBuffer(size.Width, size.Height);
            return;
        }    
        sceneBuffer.Rescale(size.Width, size.Width);
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

    public void Draw(int bufferId)
    {        
        if(sceneBuffer is null)
        {
            throw new InvalidOperationException("Frame buffer not initialized");
        }    

        sceneBuffer.Bind();
        deviceApi.ClearColor(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
        deviceApi.Clear(PGClearBufferMask.ColorBufferBit | PGClearBufferMask.DepthBufferBit, drawContext);
        DrawScene();

        deviceApi.BindFrameBuffer(bufferId);

        deviceApi.ClearColor(new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
        deviceApi.Clear(PGClearBufferMask.ColorBufferBit | PGClearBufferMask.DepthBufferBit, drawContext);
        postProcessing.Draw(sceneBuffer.GetTexture());

    }

    void DrawScene()
    {
        if (CameraComponent is null)
        {
            return;
        }

        var camera = CameraComponent;

        var projection = camera.GetProjectionMatrix((float)window.WindowSize.Width / (float)window.WindowSize.Height);
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

    public void Dispose()
    {
        OnResizeSubscription.Dispose();
        DetailsBuffer?.Dispose();
        manager.Dispose();
        meshBatcher.Dispose();
        postProcessing.Dispose();
    }
}
