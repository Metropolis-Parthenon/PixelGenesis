using CommunityToolkit.HighPerformance;
using PixelGenesis._3D.Renderer.DeviceApi.Abstractions;
using System.Numerics;
using System.Runtime.InteropServices;
using PixelGenesis.ECS;
using PixelGenesis.ECS.Helpers;

namespace PixelGenesis._3D.Renderer.DeviceObjects;

internal unsafe class RendererDeviceInstanced3DObject(
    IDeviceApi deviceApi,
    RendererDeviceMesh mesh,
    RendererDeviceMaterial material,
    RendererDeviceLightSources lightSources,
    DeviceRenderObjectManager manager) : IRendererDeviceObject
{
    int lasTransformCount = 0;

    int currentInstanceBufferSize = 0;
    IInstanceBuffer? _instanceBuffer;
    public IInstanceBuffer InstanceBuffer => _instanceBuffer ?? throw new ArgumentNullException(nameof(_instanceBuffer));
    BufferLayout _instanceBufferLayout = new BufferLayout();
    public BufferLayout InstanceBufferLayout => _instanceBufferLayout;

    int[]? materialBlockBindings;

    int[]? materialTextureBinding;

    int lightSourceBinding;

    public RendererDeviceMesh Mesh => mesh;
    public RendererDeviceMaterial Material => material;

    RendererDeviceShader? _deviceShader;
    public RendererDeviceShader DeviceShader => _deviceShader ?? throw new ArgumentNullException(nameof(_deviceShader));

    ShaderCompileContext _shaderCompileContext = new ShaderCompileContext();

    public bool ForceDataUpdate = false;
    public readonly SortedList<int, Transform3DComponent> Transforms = new SortedList<int, Transform3DComponent>();
    
    Matrix4x4[] _modelMatrixes = Array.Empty<Matrix4x4>();

    public void Initialize()
    {
        // mat4 model matrix
        _instanceBufferLayout.PushFloat(4, false);
        _instanceBufferLayout.PushFloat(4, false);
        _instanceBufferLayout.PushFloat(4, false);
        _instanceBufferLayout.PushFloat(4, false);
        
        CompileShader(lightSources.NumberOfDirLights, lightSources.NumberOfPointLights, lightSources.NumberOfSpotLights);
    }

    public void Update()
    {
        UpdateInstanceBufferData(
            UpdateBuffersSize()
            );

        if (material.IsTextureDirty ||
           lightSources.NumberOfLightChanged ||
           mesh.IsDirty)
        {            
            CompileShader(lightSources.NumberOfDirLights, lightSources.NumberOfPointLights, lightSources.NumberOfSpotLights);
        }
    }

    public void AfterUpdate() { }

    void CompileShader(int directionalLightsCount, int pointLightsCount, int spotLightsCount)
    {
        _shaderCompileContext.DeviceMesh = mesh;
        _shaderCompileContext.DirectionalLights = directionalLightsCount;
        _shaderCompileContext.PointLights = pointLightsCount;
        _shaderCompileContext.SpotLights = spotLightsCount;
        _shaderCompileContext.ModelLayout = mesh.ModelLayout;

        var compiledShader = material.CompileShader(
            _shaderCompileContext,
            out materialBlockBindings,
            out materialTextureBinding,
            out lightSourceBinding);

        var deviceShader = manager.GetOrAddDeviceShader(compiledShader);

        if (_deviceShader is not null)
        {
            manager.Return(_deviceShader);
        }

        _deviceShader = deviceShader;
    }

    void UpdateInstanceBufferData(bool bufferResized)
    {
        var transformsSpan = Transforms.ValuesAsSpan();

        bool needUpdate = false;
        for (var i = 0; i < transformsSpan.Length; ++i)
        {
            var transform = transformsSpan[i];
            needUpdate = needUpdate || transform.HasWorldChanged;
            _modelMatrixes[i] = transform.GetModelMatrix();
        }

        if(transformsSpan.Length != lasTransformCount)
        {
            needUpdate = true;
        }
        lasTransformCount = transformsSpan.Length;

        if (needUpdate || bufferResized || ForceDataUpdate)
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            _instanceBuffer.SetData(0, _modelMatrixes.AsSpan().Slice(0, Transforms.Count).AsBytes());
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }

        ForceDataUpdate = false;
    }

    bool UpdateBuffersSize()
    {
        // resize device instance buffer
        var instanceBufferSizeNeeded = Transforms.Count * sizeof(Matrix4x4);
        if (instanceBufferSizeNeeded > currentInstanceBufferSize)
        {
            currentInstanceBufferSize = Math.Max(currentInstanceBufferSize * 2, instanceBufferSizeNeeded);
            _instanceBuffer?.Dispose();
            _instanceBuffer = deviceApi.CreateInstanceBuffer(currentInstanceBufferSize, BufferHint.Dynamic);

            _modelMatrixes = new Matrix4x4[currentInstanceBufferSize / sizeof(Matrix4x4)];

            return true;
        }
        else if (_instanceBuffer is null)
        {
            _instanceBuffer = deviceApi.CreateInstanceBuffer(currentInstanceBufferSize, BufferHint.Dynamic);
            _instanceBufferLayout = new BufferLayout();

            _modelMatrixes = new Matrix4x4[currentInstanceBufferSize / sizeof(Matrix4x4)];

            return true;
        }

        return false;
    }

    public void SetShaderMaterialUniforms(
        IShaderProgram shaderProgram)
    {
        if (
            materialBlockBindings is null
            || DeviceShader is null
            || materialTextureBinding is null)
        {
            throw new InvalidOperationException("Update have to been called at least one time before this function.");
        }

        // set uniform buffer values
        for (var i = 0; i < materialBlockBindings.Length; i++)
        {
            var binding = materialBlockBindings[i];

            var uniformBuffer = Material.MaterialUniformBuffers[i];

            shaderProgram.SetUniformBlock(binding, uniformBuffer);
        }

        // set textures binding
        for (var i = 0; i < materialTextureBinding.Length; i++)
        {
            var binding = materialTextureBinding[i];
            var texture = Material.MaterialTextures[i];

            if (texture is null)
            {
                continue;
            }

            texture.DeviceTexture.SetSlot(binding);
            texture.DeviceTexture.Bind();
        }

        if (lightSourceBinding > 0)
        {
            shaderProgram.SetUniformBlock(lightSourceBinding, lightSources.LightSourceUniformBlock);
        }
    }

    public void Draw(IUniformBlockBuffer detailsBuffer, DrawContext drawContext)
    {
        drawContext.ShaderProgram = DeviceShader.ShaderProgram;
        drawContext.VertexBuffer = mesh.VertexBuffer;
        drawContext.IndexBuffer = mesh.IndexBuffer;
        drawContext.Layout = mesh.VertexBufferLayout;
        drawContext.Lenght = mesh.Mesh.Triangles.Length;
        drawContext.Offset = 0;

        SetShaderMaterialUniforms(DeviceShader.ShaderProgram);

        DeviceShader.ShaderProgram.SetUniformBlock(0, detailsBuffer);

        deviceApi.DrawTriangles(drawContext, Transforms.Count, InstanceBuffer, InstanceBufferLayout);
    }

    public void Dispose()
    {
        _instanceBuffer?.Dispose();
        manager.Return(Mesh);
        manager.Return(Material);
    }
}