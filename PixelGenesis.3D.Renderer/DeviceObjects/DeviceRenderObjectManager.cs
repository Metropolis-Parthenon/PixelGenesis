using PixelGenesis._3D.Common;
using PixelGenesis._3D.Renderer.DeviceApi.Abstractions;
using PixelGenesis.ECS;

namespace PixelGenesis._3D.Renderer.DeviceObjects;

internal class DeviceRenderObjectManager(IDeviceApi deviceApi, PGScene scene) : IDisposable
{
    RendererDeviceLightSources lightSources = new RendererDeviceLightSources(deviceApi, scene).Init();
        
    readonly SortedList<Guid, RefCounted<RendererDeviceTexture>> textureObjects = new();
    readonly SortedList<Guid, RefCounted<RendererDeviceMesh>> meshObjects = new();
    readonly SortedList<Guid, RefCounted<RendererDeviceShader>> compiledShaderObjects = new();
    readonly SortedList<Guid, RefCounted<RendererDeviceMaterial>> materialObjects = new();
    readonly SortedList<(Guid, Guid), RendererDeviceInstanced3DObject> instancedObjects = new();

    public RendererDeviceInstanced3DObject GetOrAddInstanced3DObject(Material material, IMesh mesh)
    {
        ref var result = ref instancedObjects.GetValueRefOrAddDefault((material.Id, mesh.Id), out var exists);
        if (!exists) 
        {
            result = new RendererDeviceInstanced3DObject(deviceApi, GetOrAddMesh(mesh), GetOrAddMaterial(material), lightSources, this);
            result.Initialize();
        }

#pragma warning disable CS8603 // Possible null reference return.
        return result;
#pragma warning restore CS8603 // Possible null reference return.
    }

    public RendererDeviceMaterial GetOrAddMaterial(Material material) 
    {
        ref var result = ref materialObjects.GetValueRefOrAddDefault(material.Id, out var exists);
        if (!exists)
        {
            result = new RefCounted<RendererDeviceMaterial>(new RendererDeviceMaterial(deviceApi, material, this));
            result.Value.Initialize();
        }

#pragma warning disable CS8603 // Possible null reference return.
        result.Increase();
        return result.Value;
#pragma warning restore CS8603 // Possible null reference return.
    }

    public RendererDeviceMesh GetOrAddMesh(IMesh mesh) 
    {
        ref var result = ref meshObjects.GetValueRefOrAddDefault(mesh.Id, out var exists);
        if (!exists)
        {
            result = new RefCounted<RendererDeviceMesh>(new RendererDeviceMesh(deviceApi, mesh));
            result.Value.Initialize();
        }

#pragma warning disable CS8603 // Possible null reference return.
        result.Increase();
        return result.Value;
#pragma warning restore CS8603 // Possible null reference return.
    }

    public RendererDeviceShader GetOrAddDeviceShader(CompiledShader compiledShader) 
    {
        ref var result = ref compiledShaderObjects.GetValueRefOrAddDefault(compiledShader.Id, out var exists);
        if (!exists)
        {
            result = new RefCounted<RendererDeviceShader>(new RendererDeviceShader(deviceApi, compiledShader));
            result.Value.Initialize();
        }

#pragma warning disable CS8603 // Possible null reference return.
        result.Increase();
        return result.Value;
#pragma warning restore CS8603 // Possible null reference return.
    }

    public RendererDeviceTexture GetOrAddDeviceTexture(Texture texture) 
    {
        ref var result = ref textureObjects.GetValueRefOrAddDefault(texture.Id, out var exists);
        if (!exists)
        {
            result = new RefCounted<RendererDeviceTexture>(new RendererDeviceTexture(deviceApi, texture));
            result.Value.Initialize();
        }

#pragma warning disable CS8603 // Possible null reference return.
        result.Increase();
        return result.Value;
#pragma warning restore CS8603 // Possible null reference return.
    }

    public Span<RendererDeviceInstanced3DObject> InstanceObjects => instancedObjects.ValuesAsSpan();

    public void Destroy(RendererDeviceInstanced3DObject instancedObject)
    {
        instancedObject.Dispose();
        instancedObjects.Remove((instancedObject.Material.Material.Id, instancedObject.Mesh.Mesh.Id));
    }

    public void Return(RendererDeviceTexture val)
    {
        if(textureObjects[val.Texture.Id].Decrease())
        {
            val.Dispose();
            textureObjects.Remove(val.Texture.Id);
        }
    }

    public void Return(RendererDeviceMesh val)
    {
        if (meshObjects[val.Mesh.Id].Decrease())
        {
            val.Dispose();
            meshObjects.Remove(val.Mesh.Id);
        }
    }

    public void Return(RendererDeviceShader val)
    {
        if (compiledShaderObjects[val.CompiledShader.Id].Decrease())
        {
            val.Dispose();
            compiledShaderObjects.Remove(val.CompiledShader.Id);
        }
    }

    public void Return(RendererDeviceMaterial val)
    {
        if (materialObjects[val.Material.Id].Decrease())
        {
            val.Dispose();
            materialObjects.Remove(val.Material.Id);
        }
    }

    public void Update()
    {
        lightSources.Update();

        var textureObjectsSpan = textureObjects.ValuesAsSpan();
        for (var i = 0; i < textureObjectsSpan.Length; i++)
        {
            textureObjectsSpan[i].Value.Update();
        }

        var meshObjectsSpan = meshObjects.ValuesAsSpan();
        for(var i = 0; i < meshObjectsSpan.Length; i++)
        {
            meshObjectsSpan[i].Value.Update();
        }

        var compiledShaderObjectsSpan = compiledShaderObjects.ValuesAsSpan();
        for (var i = 0; i < compiledShaderObjectsSpan.Length; i++)
        {
            compiledShaderObjectsSpan[i].Value.Update();
        }

        var materialObjectsSpan = materialObjects.ValuesAsSpan();
        for (var i = 0; i < materialObjectsSpan.Length; i++)
        {
            materialObjectsSpan[i].Value.Update();
        }

        var instanceObjectsSpan = instancedObjects.ValuesAsSpan();
        for(var i = 0; i < instanceObjectsSpan.Length; i++)
        {
            instanceObjectsSpan[i].Update();
        }

        // After update
        lightSources.AfterUpdate();

        for (var i = 0; i < textureObjectsSpan.Length; i++)
        {
            textureObjectsSpan[i].Value.AfterUpdate();
        }

        for (var i = 0; i < meshObjectsSpan.Length; i++)
        {
            meshObjectsSpan[i].Value.AfterUpdate();
        }

        for (var i = 0; i < compiledShaderObjectsSpan.Length; i++)
        {
            compiledShaderObjectsSpan[i].Value.AfterUpdate();
        }

        for (var i = 0; i < materialObjectsSpan.Length; i++)
        {
            materialObjectsSpan[i].Value.AfterUpdate();
        }

        for (var i = 0; i < instanceObjectsSpan.Length; i++)
        {
            instanceObjectsSpan[i].AfterUpdate();
        }
    }

    public void Dispose()
    {
        lightSources.Dispose();
        foreach (var val in instancedObjects.Values) 
        {
            val.Dispose();   
        }
        foreach (var val in materialObjects.Values)
        {
            val.Value.Dispose();
        }
        foreach (var val in meshObjects.Values)
        {
            val.Value.Dispose();
        }
        foreach (var val in compiledShaderObjects.Values)
        {
            val.Value.Dispose();
        }
        foreach (var val in textureObjects.Values)
        {
            val.Value.Dispose();
        }
    }
}

internal struct RefCounted<T>(T value) where T : IRendererDeviceObject
{    
    public T Value => value;

    int referencesCount = 0;
    public int ReferenceCount => referencesCount;

    public void Increase()
    {
        referencesCount++;
    }

    public bool Decrease() 
    {
        referencesCount--;
        return referencesCount == 0;
    }
}