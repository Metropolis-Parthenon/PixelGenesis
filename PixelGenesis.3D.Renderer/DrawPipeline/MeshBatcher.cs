using PixelGenesis._3D.Common;
using PixelGenesis._3D.Common.Components;
using PixelGenesis.ECS;
using PixelGenesis.ECS.Helpers;
using PixelGenesis.ECS.Scene;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PixelGenesis._3D.Renderer.DrawPipeline;

internal class MeshBatcher(PGScene scene, ChangesTracker changesTracker) : IDisposable
{
    SortedList<int, Batch> _batches = new();
    public ReadOnlySpan<Batch> Batches => _batches.ValuesAsSpan();

    List<Batch> removedBatches = new();
    public ReadOnlySpan<Batch> RemovedBatches => CollectionsMarshal.AsSpan(removedBatches);

    List<Batch> addedBatches = new();
    public ReadOnlySpan<Batch> AddedBatches => CollectionsMarshal.AsSpan(addedBatches);

    public void Initialize()
    {
        var meshes = scene.GetComponents<MeshRendererComponent>();
        for (int i = 0; i < meshes.Length; i++) 
        { 
            var meshRenderer = Unsafe.As<MeshRendererComponent>(meshes[i]);
            if(meshRenderer.Material is null || meshRenderer.Mesh is null)
            {
                continue;
            }
            _batches.TryAdd(meshRenderer.Entity.Id, new Batch(meshRenderer.Mesh, meshRenderer.Material, meshRenderer.Transform));
        }
    }

    public void Update()
    {
        var addedMeshes = changesTracker.AddedMeshComponents;

        for (var i = 0; i < addedMeshes.Length; i++)
        {
            var meshRenderer = addedMeshes[i];
            if (meshRenderer.Material is null || meshRenderer.Mesh is null)
            {
                continue;
            }

            var batch = new Batch(meshRenderer.Mesh, meshRenderer.Material, meshRenderer.Transform);
            if (_batches.TryAdd(meshRenderer.Entity.Id, batch))
            {
                addedBatches.Add(batch);
            }
        }

        var removedMeshes = changesTracker.RemovedMeshComponents;

        for(var i = 0;i < removedMeshes.Length; i++)
        {
            var meshRenderer = removedMeshes[i];
            if (meshRenderer.Material is null || meshRenderer.Mesh is null)
            {
                continue;
            }

            if(_batches.Remove(meshRenderer.Entity.Id, out var batch))
            {
                removedBatches.Add(batch);
            }
        }
    }

    public void AfterUpdate()
    {
        addedBatches.Clear();
        removedBatches.Clear();
    }

    public void Dispose()
    {
        
    }
}

internal struct Batch(IMesh mesh, Material material, Transform3DComponent transform)
{
    public IMesh Mesh => mesh;
    public Material Material => material;
    public Transform3DComponent Transform => transform;
}