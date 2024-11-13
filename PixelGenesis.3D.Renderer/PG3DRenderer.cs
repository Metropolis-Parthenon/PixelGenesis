using CommunityToolkit.HighPerformance;
using PixelGenesis._3D.Common;
using PixelGenesis._3D.Common.Components;
using PixelGenesis._3D.Renderer.DeviceApi.Abstractions;
using PixelGenesis.ECS;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PixelGenesis._3D.Renderer;
public class PG3DRenderer(IDeviceApi deviceApi, IPGWindow pGWindow, EntityManager entityManager) : IDisposable
{
    Dictionary<Guid, IShaderProgram> DeviceShaders = new Dictionary<Guid, IShaderProgram>(50);
    SortedList<Guid, KeyValuePair<int, (IUniformBlockBuffer Buffer, int Count)>[]> MaterialData = new SortedList<Guid, KeyValuePair<int, (IUniformBlockBuffer Buffer, int Count)>[]>(200);

    SortedList<(Guid, Guid), InstancedObjects> InstancedObjects = new SortedList<(Guid, Guid), InstancedObjects>(1000);
        
    PerspectiveCameraComponent? CameraComponent;

    // we can use this buffer to send any data we want from the renderer to the shader
    // for now we are only sending the projection * view matrix
    // later we can send more data like light positions, time passed, etc
    IUniformBlockBuffer DetailsBuffer = deviceApi.CreateUniformBlockBuffer<Matrix4x4>(BufferHint.Dynamic);

    DrawContext drawContext = new DrawContext()
    {
        
    };

    public unsafe void Update()
    {
        var cameras = entityManager.GetComponents<PerspectiveCameraComponent>();
        if(cameras.Length == 0)
        {
            return;
        }
        // for now we use the first camera we find
        CameraComponent = Unsafe.As<PerspectiveCameraComponent>(cameras[0]);

        var instancedObjectsToRemove = new HashSet<(Guid, Guid)>(InstancedObjects.Count);
        var instanceObjects = InstancedObjects.ValuesAsSpan();
        for(var i = 0; i < instanceObjects.Length; ++i)
        {
            instanceObjects[i].Transforms.Clear();
            instancedObjectsToRemove.Add((instanceObjects[i].Mesh.Id, instanceObjects[i].Material.Id));
        }

        var meshRendererComponents = entityManager.GetComponents<MeshRendererComponent>();

        for(var i = 0; i < meshRendererComponents.Length; i++)
        {
            var meshRenderer = Unsafe.As<MeshRendererComponent>(meshRendererComponents[i]);

            var mesh = meshRenderer.Mesh;
            var material = meshRenderer.Material;
            var transform = meshRenderer.GetTransform();

            if(mesh is null || material is null || transform is null)
            {
                continue;
            }

            var key = (mesh.Id, material.Id);

            ref var instancedObject = ref InstancedObjects.GetValueRefOrAddDefault(key, out bool instancedObjectExisted);

            if(!instancedObjectExisted)
            {
                instancedObject = new InstancedObjects(deviceApi, mesh, material);                
            }

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            instancedObject.Transforms.Add(transform);
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            instancedObjectsToRemove.Remove(key);
        }

        // update instance objects
        instanceObjects = InstancedObjects.ValuesAsSpan();
        for (var i = 0; i < instanceObjects.Length; ++i)
        {
            instanceObjects[i].Update();         
        }

        UpdateMaterials();
    }

    unsafe void UpdateMaterials()
    {
        //update material data
        Span<float> floatSpan = stackalloc float[1];
        Span<Vector2> vector2Span = stackalloc Vector2[1];
        Span<Vector3> vector3Span = stackalloc Vector3[1];
        Span<Vector4> vector4Span = stackalloc Vector4[1];
        Span<Matrix4x4> matrix4x4Span = stackalloc Matrix4x4[1];

        var materialDataToRemove = new HashSet<Guid>(MaterialData.Count);

        var materialDataKeys = MaterialData.KeysAsSpan();
        foreach (var key in materialDataKeys)
        {
            materialDataToRemove.Add(key);
        }

        var instanceObjects = InstancedObjects.ValuesAsSpan();

        var visitedMaterials = new HashSet<Guid>();
        for (var i = 0; i < instanceObjects.Length; ++i)
        {
            var material = instanceObjects[i].Material;
            if (visitedMaterials.Contains(material.Id))
            {
                continue;
            }

            materialDataToRemove.Remove(material.Id);

            ref var value = ref MaterialData.GetValueRefOrAddDefault(material.Id, out bool materialDataExisted);
            if (!materialDataExisted)
            {
                var shaderBlocks = CollectionsMarshal.AsSpan(material.Shader.Layout.Blocks);
                value = new KeyValuePair<int, (IUniformBlockBuffer Buffer, int Count)>[shaderBlocks.Length];

                for (var j = 0; j < shaderBlocks.Length; j++)
                {
                    var blockLayout = shaderBlocks[j];

                    var sizes = blockLayout.Parameters.Select(p => p.Type switch
                    {
                        Common.Type.Float => sizeof(float),
                        Common.Type.Float2 => sizeof(Vector2),
                        Common.Type.Float3 => sizeof(Vector3),
                        Common.Type.Float4 => sizeof(Vector4),
                        Common.Type.Color4 => sizeof(Vector4),
                        Common.Type.Mat4 => sizeof(Matrix4x4),
                        _ => throw new Exception("Unsupported type")
                    }).ToArray();

                    value[j] = new KeyValuePair<int, (IUniformBlockBuffer Buffer, int Count)>(
                        blockLayout.Binding,
                        (deviceApi.CreateUniformBlockBuffer(sizes, BufferHint.Dynamic), sizes.Length));
                }
            }

            for (var j = 0; j < value.Length; ++j)
            {
                var binding = value[j].Key;
                var buffer = value[j].Value.Buffer;
                var count = value[j].Value.Count;

                for (var k = 0; k < count; ++k)
                {
                    var parameter = material.GetParameter(binding, k);

                    Span<byte> data;
                    switch (parameter)
                    {
                        case float f:
                            floatSpan[0] = f;
                            data = floatSpan.AsBytes();
                            break;
                        case Vector2 v:
                            vector2Span[0] = v;
                            data = vector2Span.AsBytes();
                            break;
                        case Vector3 vector3:
                            vector3Span[0] = vector3;
                            data = vector3Span.AsBytes();
                            break;
                        case Vector4 vector4:
                            vector4Span[0] = vector4;
                            data = vector4Span.AsBytes();
                            break;
                        case Matrix4x4 matrix4x4:
                            matrix4x4Span[0] = matrix4x4;
                            data = matrix4x4Span.AsBytes();
                            break;
                        default:
                            throw new Exception("Unsupported type");
                    }

                    buffer.SetData(data, k);
                }
            }

            visitedMaterials.Add(material.Id);
        }

        foreach(var key in materialDataToRemove)
        {
            var materialData = MaterialData[key];
            for(var i = 0; i < materialData.Length; ++i)
            {
                materialData[i].Value.Buffer.Dispose();
            }

            MaterialData.Remove(key);
        }
    }

    public void Render()
    {
        if(CameraComponent is null)
        {
            return;
        }

        var viewProjection = CameraComponent.GetProjectionMatrix(pGWindow.Width / pGWindow.Height) * CameraComponent.GetViewMatrix();

        DetailsBuffer.SetData(viewProjection, 0);

        var instancedObjects = InstancedObjects.ValuesAsSpan();

        for(var i = 0; i < instancedObjects.Length; ++i)
        {
            var instancedObject = instancedObjects[i];

            drawContext.VertexBuffer = instancedObject.VertexBuffer;
            drawContext.IndexBuffer = instancedObject.IndexBuffer;
            drawContext.Layout = instancedObject.VertexBufferLayout;

            var shaderProgram = CollectionsMarshal.GetValueRefOrAddDefault(DeviceShaders, instancedObject.Material.Shader.Id, out bool shaderProgramExisted);
            if(!shaderProgramExisted)
            {
                var compiledShader = instancedObject.Material.Shader;
                shaderProgram = deviceApi.CreateShaderProgram(
                    compiledShader.Vertex, 
                    compiledShader.Fragment, 
                    compiledShader.Tessellation, 
                    compiledShader.Geometry);
            }

#pragma warning disable CS8601 // Possible null reference assignment.
            drawContext.ShaderProgram = shaderProgram;
#pragma warning restore CS8601 // Possible null reference assignment.

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            shaderProgram.SetUniformBlock(0, DetailsBuffer);
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            var materialBuffers = MaterialData[instancedObject.Material.Id];
            for(var j = 0; j < materialBuffers.Length; j++)
            {
                var buffer = materialBuffers[j].Value.Buffer;
                shaderProgram.SetUniformBlock(materialBuffers[j].Key, buffer);
            }

            deviceApi.DrawTriangles(drawContext, instancedObject.Transforms.Count, instancedObject.InstanceBuffer, instancedObject.InstanceBufferLayout);
        }
    }

    public void Dispose()
    {
        foreach(var shader in DeviceShaders)
        {
            shader.Value.Dispose();
        }

        foreach (var item in InstancedObjects)
        {
            item.Value.Dispose();
        }
    }
}

public class InstancedObjects : IDisposable
{
    public readonly IMesh Mesh;
    public readonly Material Material;

    public IVertexBuffer VertexBuffer { get; private set; }
    public VertexBufferLayout VertexBufferLayout { get; private set; }
    public IIndexBuffer IndexBuffer { get; private set; }
    public IInstanceBuffer InstanceBuffer { get; private set; }
    public VertexBufferLayout InstanceBufferLayout { get; }

    public readonly List<Transform3DComponent> Transforms = new List<Transform3DComponent>();

    IDeviceApi _deviceApi;
    int currentInstanceBufferSize;

    public unsafe InstancedObjects(IDeviceApi deviceApi, IMesh mesh, Material material)
    {
        _deviceApi = deviceApi;
        currentInstanceBufferSize = 50 * sizeof(Matrix4x4);

        Mesh = mesh;
        Material = material;
        
        InstanceBuffer = deviceApi.CreateInstanceBuffer(currentInstanceBufferSize, BufferHint.Dynamic);
        InstanceBufferLayout = new VertexBufferLayout();

        // 4*4 model matrix
        InstanceBufferLayout.PushFloat(4, false);
        InstanceBufferLayout.PushFloat(4, false);
        InstanceBufferLayout.PushFloat(4, false);
        InstanceBufferLayout.PushFloat(4, false);

        UpdateMesh();
    }

    public unsafe void Update()
    {
        var modelMats = new Matrix4x4[Transforms.Count];
        if(modelMats.Length * sizeof(Matrix4x4) > currentInstanceBufferSize)
        {
            currentInstanceBufferSize = Math.Max(currentInstanceBufferSize * 2, modelMats.Length * sizeof(Matrix4x4));
            InstanceBuffer.Dispose();
            InstanceBuffer = _deviceApi.CreateInstanceBuffer(currentInstanceBufferSize, BufferHint.Dynamic);
        }

        var transformsSpan = CollectionsMarshal.AsSpan(Transforms);

        for(var i = 0; i < transformsSpan.Length; ++i)
        {
            modelMats[i] = transformsSpan[i].GetModelMatrix();
        }

        InstanceBuffer.SetData(0, modelMats.AsSpan().AsBytes());

        if(Mesh.IsDirty)
        {
            UpdateMesh();
        }
    }

    void UpdateMesh()
    {
        VertexBuffer?.Dispose();
        VertexBuffer = _deviceApi.CreateVertexBuffer(Mesh.Vertices.AsBytes(), BufferHint.Static);
        VertexBufferLayout = new VertexBufferLayout();
        VertexBufferLayout.PushFloat(3, false);

        IndexBuffer?.Dispose();
        IndexBuffer = _deviceApi.CreateIndexBuffer(Mesh.Triangles, BufferHint.Static);
    }

    public void Dispose()
    {
        VertexBuffer.Dispose();
        IndexBuffer.Dispose();
        InstanceBuffer.Dispose();
    }
}