using PixelGenesis._3D.Common.Components;
using PixelGenesis._3D.Renderer.DeviceObjects;
using PixelGenesis.ECS.Scene;
using System.Runtime.CompilerServices;

namespace PixelGenesis._3D.Renderer.DrawPipeline;

internal class MaterialLoader(PGScene pGScene, ChangesTracker changesTracker, DeviceRenderObjectManager manager)
{
    public void Initialize()
    {
        var meshRenderers = pGScene.GetComponents<MeshRendererComponent>();

        for(var i = 0; i < meshRenderers.Length; i++)
        {
            var component = Unsafe.As<MeshRendererComponent>(meshRenderers[i]);

            if(component.Material is null)
            {
                continue;
            }

            manager.GetOrAddMaterial(component.Material);            
        }

    }

    public void Update()
    {
        var addedMeshes = changesTracker.AddedMeshComponents;
        for (var i = 0; i < addedMeshes.Length; i++)
        { 
            var component = addedMeshes[i];

            if(component.Material is null)
            {
                continue;
            }

            manager.GetOrAddMaterial(component.Material);
        }

        var removedMeshes = changesTracker.RemovedMeshComponents;
        for(var i = 0; i < removedMeshes.Length; i++)
        {
            var component = removedMeshes[i];

            if(component.Material is null)
            {
                continue;
            }

            // TODO remove material
        }

    }
}
