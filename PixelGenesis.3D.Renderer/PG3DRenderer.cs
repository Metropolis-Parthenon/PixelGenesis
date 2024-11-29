using CommunityToolkit.HighPerformance;
using PixelGenesis._3D.Common;
using PixelGenesis._3D.Common.Components;
using PixelGenesis._3D.Common.Components.Lighting;
using PixelGenesis._3D.Renderer.DeviceApi.Abstractions;
using PixelGenesis._3D.Renderer.DeviceObjects;
using PixelGenesis.ECS;
using PixelGenesis.ECS.Components;
using PixelGenesis.ECS.Scene;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace PixelGenesis._3D.Renderer;
public class PG3DRenderer(IDeviceApi deviceApi, IPGWindow pGWindow, PGScene scene) : IDisposable
{
    public readonly RendererMetrics Metrics = new RendererMetrics();
    Stopwatch sw = new Stopwatch();

    PerspectiveCameraComponent? CameraComponent;

    DeviceRenderObjectManager deviceObjectManager = new DeviceRenderObjectManager(deviceApi, scene);

    // we can use this buffer to send any data we want from the renderer to the shader
    // for now we are only sending the projection * view matrix
    // later we can send more data like light positions, time passed, etc
    IUniformBlockBuffer DetailsBuffer;

    DrawContext drawContext = new DrawContext()
    {
        EnableDepthTest = true,
    };
        
    public unsafe void Initialize()
    {
        DetailsBuffer = deviceApi.CreateUniformBlockBuffer<Matrix4x4, Vector3>(BufferHint.Dynamic);

        UpdateComponents();

        var cameras = scene.GetComponents<PerspectiveCameraComponent>();
        if (cameras.Length == 0)
        {
            return;
        }
        // for now we use the first camera we find
        CameraComponent = Unsafe.As<PerspectiveCameraComponent>(cameras[0]);

        var meshRendererComponents = scene.GetComponents<MeshRendererComponent>();

        for (var i = 0; i < meshRendererComponents.Length; i++)
        {
            var meshRenderer = Unsafe.As<MeshRendererComponent>(meshRendererComponents[i]);

            var mesh = meshRenderer.Mesh;
            var material = meshRenderer.Material;
            var transform = meshRenderer.GetTransform();

            if (mesh is null || material is null || transform is null)
            {
                continue;
            }

            var instancedObject = deviceObjectManager.GetOrAddInstanced3DObject(material, mesh);

            instancedObject.Transforms.Add(transform);            
        }

    }

    public unsafe void Update()
    {
        sw.Reset();
        sw.Start();

        sw.Stop();
        Metrics.UpdateTime = sw.Elapsed.TotalMilliseconds;
    }

    bool jojoto = true;
    void UpdateComponents()
    {
        if (jojoto)
        {
            jojoto = false;
            //update all components
            var allEntities = scene.Entities;
            for (var i = 0; i < allEntities.Length; i++)
            {
                var entity = allEntities[i];
                var components = entity.Components;
                for (var j = 0; j < components.Length; j++)
                {
                    var component = components[j];
                    if (component is IUpdate updateComponent)
                    {
                        updateComponent.Update();
                    }
                    if (component is Transform3DComponent transform && transform.Entity.Parent is null)
                    {
                        transform.UpdateModelMatrix();
                    }
                }
            }
        }
    }

    public void Render()
    {
        deviceObjectManager.Update();

        sw.Reset();
        sw.Start();

        if (CameraComponent is null)
        {
            return;
        }

        var projection = CameraComponent.GetProjectionMatrix(pGWindow.Width / pGWindow.Height);
        var view = CameraComponent.GetViewMatrix();

        var viewProjection = view * projection;

        DetailsBuffer.SetData(viewProjection, 0);
        DetailsBuffer.SetData(CameraComponent.GetTransform().Position, 1);

        Metrics.DrawCalls = 0;
        var instancedObjects = deviceObjectManager.InstanceObjects;
        for (var i = 0; i < instancedObjects.Length; ++i)
        {
            var instancedObject = instancedObjects[i];

            instancedObject.Draw(DetailsBuffer, drawContext);

            Metrics.DrawCalls++;
        }

        RenderSkybox(view, projection);
        sw.Stop();

        Metrics.RenderTime = sw.Elapsed.TotalMilliseconds;
    }

    public void RenderSkybox(Matrix4x4 view, Matrix4x4 projection)
    {
        if (CameraComponent is null)
        {
            return;
        }

        var renderer = CameraComponent.GetSkyboxRenderer(deviceApi);

        if (renderer is null)
        {
            return;
        }

        renderer.DrawSkybox(projection, view);
    }

    public void Dispose()
    {
        deviceObjectManager.Dispose();
    }
}

public class RendererMetrics
{
    public double UpdateTime;
    public double RenderTime;
    public int DrawCalls;
}