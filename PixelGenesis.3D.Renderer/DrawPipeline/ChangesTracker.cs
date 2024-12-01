using PixelGenesis._3D.Common.Components;
using PixelGenesis.ECS.Components;
using PixelGenesis.ECS;
using PixelGenesis.ECS.Scene;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace PixelGenesis._3D.Renderer.DrawPipeline;

public class ChangesTracker(PGScene scene) : IDisposable
{
    IDisposable? addedMeshSubscription;
    IDisposable? removedMeshSubscription;
    IDisposable? addedCameraSubscription;
    IDisposable? removeCameraSubscription;

    List<MeshRendererComponent> _addedMeshComponents = new List<MeshRendererComponent>();
    List<MeshRendererComponent> _removedMeshComponents = new List<MeshRendererComponent>();
    List<PerspectiveCameraComponent> _cameras = new List<PerspectiveCameraComponent>();

    public ReadOnlySpan<MeshRendererComponent> AddedMeshComponents => CollectionsMarshal.AsSpan(_addedMeshComponents);
    public ReadOnlySpan<MeshRendererComponent> RemovedMeshComponents => CollectionsMarshal.AsSpan(_removedMeshComponents);

    public ReadOnlySpan<PerspectiveCameraComponent> Cameras => CollectionsMarshal.AsSpan(_cameras);

    public void Initialize()
    {
        addedMeshSubscription = 
            scene
            .ComponentAdded
            .Where(x => x.Component is  MeshRendererComponent)
            .Select(x => x.Component)
            .Cast<MeshRendererComponent>()
            .Subscribe(_addedMeshComponents.Add);

        removedMeshSubscription =
            scene
            .ComponentRemoved
            .Where(x => x.Component is MeshRendererComponent)
            .Select(x => x.Component)
            .Cast<MeshRendererComponent>()
            .Subscribe(_addedMeshComponents.Add);

        addedCameraSubscription =
            scene
            .ComponentAdded
            .Where(x => x.Component is PerspectiveCameraComponent)
            .Select(x => x.Component)
            .Cast<PerspectiveCameraComponent>()
            .Subscribe(_cameras.Add);

        removeCameraSubscription =
            scene
            .ComponentAdded
            .Where(x => x.Component is PerspectiveCameraComponent)
            .Select(x => x.Component)
            .Cast<PerspectiveCameraComponent>()
            .Subscribe(x => _cameras.Remove(x));

        var cameras = scene.GetComponents<PerspectiveCameraComponent>();

        for (int i = 0; i < cameras.Length; i++) 
        {
            _cameras.Add(Unsafe.As<PerspectiveCameraComponent>(cameras[i]));
        }

        Update();
    }

    public void Update()
    {
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

    public void AfterUpdate()
    {
        _addedMeshComponents.Clear();
        _removedMeshComponents.Clear();
    }

    public void Dispose()
    {
        addedMeshSubscription?.Dispose();
        removedMeshSubscription?.Dispose();
        addedCameraSubscription?.Dispose();
        removeCameraSubscription?.Dispose();
    }
}
