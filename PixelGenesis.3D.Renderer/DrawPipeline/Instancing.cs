using PixelGenesis._3D.Renderer.DeviceObjects;

namespace PixelGenesis._3D.Renderer.DrawPipeline;

internal class Instancing(MeshBatcher meshBatcher, DeviceRenderObjectManager manager)
{
    public void Initialize()
    {
        var batches = meshBatcher.Batches;
        for (var i = 0; i < batches.Length; i++) 
        {
            var batch = batches[i];
            var instanced3DObject = manager.GetOrAddInstanced3DObject(batch.Material, batch.Mesh);

            instanced3DObject.Transforms.Add(batch.Transform.Entity.Id, batch.Transform);
        }
    }

    public void Update()
    {
        var addedBatches = meshBatcher.AddedBatches;
        for(var i = 0; i < addedBatches.Length;i++)
        {
            var batch = addedBatches[i];

            var instanced3DObject = manager.GetOrAddInstanced3DObject(batch.Material, batch.Mesh);
            instanced3DObject.Transforms.Add(batch.Transform.Entity.Id, batch.Transform);
            instanced3DObject.ForceDataUpdate = true;
        }
        
        var removedBatches = meshBatcher.RemovedBatches;
        for(var i = 0; i < removedBatches.Length;i++)
        {
            var batch = removedBatches[i];

            var instanced3DObject = manager.GetOrAddInstanced3DObject(batch.Material, batch.Mesh);
            instanced3DObject.Transforms.Remove(batch.Transform.Entity.Id);
            instanced3DObject.ForceDataUpdate = true;
        }
    }
}
