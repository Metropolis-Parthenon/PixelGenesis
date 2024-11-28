using CommunityToolkit.HighPerformance;
using PixelGenesis._3D.Common.Components.Lighting;
using PixelGenesis._3D.Renderer.DeviceApi.Abstractions;
using PixelGenesis.ECS;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;


namespace PixelGenesis._3D.Renderer.DeviceObjects;

internal class RendererDeviceLightSources(IDeviceApi deviceApi, PGScene scene) : IRendererDeviceObject
{
    public IUniformBlockBuffer LightSourceUniformBlock { get; private set; }

    public bool NumberOfLightChanged { get; private set; }

    int _lastDirLightsLength;
    int _lastPointLightsLength;
    int _lastSpotLightsLength;

    public int NumberOfDirLights => _lastDirLightsLength;
    public int NumberOfPointLights => _lastPointLightsLength;
    public int NumberOfSpotLights => _lastSpotLightsLength;

    public unsafe void Initialize()
    {
        var directionalLightComponents = scene.GetComponents<DirectionalLightComponent>();
        var pointLightComponents = scene.GetComponents<PointLightComponent>();
        var spotLightComponents = scene.GetComponents<SpotLightComponent>();

        CreateBuffer(directionalLightComponents.Length, pointLightComponents.Length, spotLightComponents.Length);

        Span<DirLight> dirLights = stackalloc DirLight[directionalLightComponents.Length];
        Span<PointLight> pointLights = stackalloc PointLight[pointLightComponents.Length];
        Span<SpotLight> spotLights = stackalloc SpotLight[spotLightComponents.Length];

        for (var i = 0; i < directionalLightComponents.Length; i++)
        {
            var component = Unsafe.As<DirectionalLightComponent>(directionalLightComponents[i]);
            dirLights[i] = new DirLight()
            {
                Color = component.Color,
                Direction = component.Transform.Position,
                Intensity = component.Intensity,
            };
        }

        for (var i = 0; i < pointLightComponents.Length; i++)
        {
            var component = Unsafe.As<PointLightComponent>(pointLightComponents[i]);
            pointLights[i] = new PointLight()
            {
                Color = component.Color,
                Position = component.Transform.Position,
                Intensity = component.Intensity
            };
        }

        for (var i = 0; i < spotLightComponents.Length; i++)
        {
            var component = Unsafe.As<SpotLightComponent>(spotLightComponents[i]);
            spotLights[i] = new SpotLight()
            {
                Color = component.Color,
                Position = component.Transform.Position,
                Intensity = component.Intensity,
                CutOff = component.CutOff,
                Direction = component.Transform.Position
            };
        }


        int index = 0;
        // set light sources
        if (dirLights.Length > 0)
        {
            LightSourceUniformBlock?.SetData(dirLights.AsBytes(), index);
            index++;
        }

        if (pointLights.Length > 0)
        {
            LightSourceUniformBlock?.SetData(pointLights.AsBytes(), index);
            index++;
        }

        if (spotLights.Length > 0)
        {
            LightSourceUniformBlock?.SetData(spotLights.AsBytes(), index);
            index++;
        }
    }

    public void Update()
    {
        var directionalLightComponents = scene.GetComponents<DirectionalLightComponent>();
        var pointLightComponents = scene.GetComponents<PointLightComponent>();
        var spotLightComponents = scene.GetComponents<SpotLightComponent>();

        if(
            directionalLightComponents.Length != _lastDirLightsLength ||
            pointLightComponents.Length != _lastPointLightsLength ||
            spotLightComponents.Length != _lastSpotLightsLength)
        {
            LightSourceUniformBlock.Dispose();
            CreateBuffer(directionalLightComponents.Length, pointLightComponents.Length, spotLightComponents.Length);
        }


        Span<DirLight> dirLights = stackalloc DirLight[directionalLightComponents.Length];
        Span<PointLight> pointLights = stackalloc PointLight[pointLightComponents.Length];
        Span<SpotLight> spotLights = stackalloc SpotLight[spotLightComponents.Length];

        var dirLightsChanged = false;
        for (var i = 0; i < directionalLightComponents.Length; i++)
        {
            var component = Unsafe.As<DirectionalLightComponent>(directionalLightComponents[i]);
            dirLights[i] = new DirLight()
            {
                Color = component.Color,
                Direction = component.Transform.Position,
                Intensity = component.Intensity,
            };

            dirLightsChanged = dirLightsChanged || component.Transform.HasWorldChanged;
        }

        var pointLightsChanged = false;
        for (var i = 0; i < pointLightComponents.Length; i++)
        {
            var component = Unsafe.As<PointLightComponent>(pointLightComponents[i]);
            pointLights[i] = new PointLight()
            {
                Color = component.Color,
                Position = component.Transform.Position,
                Intensity = component.Intensity
            };

            pointLightsChanged = pointLightsChanged || component.Transform.HasWorldChanged;
        }

        var spotLightChanged = false;
        for (var i = 0; i < spotLightComponents.Length; i++)
        {
            var component = Unsafe.As<SpotLightComponent>(spotLightComponents[i]);
            spotLights[i] = new SpotLight()
            {
                Color = component.Color,
                Position = component.Transform.Position,
                Intensity = component.Intensity,
                CutOff = component.CutOff,
                Direction = component.Transform.Position
            };

            spotLightChanged = spotLightChanged || component.Transform.HasWorldChanged;
        }


        int index = 0;
        // set light sources
        if (dirLights.Length > 0)
        {
            if(dirLightsChanged)
            {
                LightSourceUniformBlock?.SetData(dirLights.AsBytes(), index);
            }
            
            index++;
        }

        if (pointLights.Length > 0)
        {
            if (pointLightsChanged) 
            {
                LightSourceUniformBlock?.SetData(pointLights.AsBytes(), index);
            }
            
            index++;
        }

        if (spotLights.Length > 0)
        {
            if(spotLightChanged)
            {
                LightSourceUniformBlock?.SetData(spotLights.AsBytes(), index);
            }
            
            index++;
        }
    }

    unsafe void CreateBuffer(int dirLightLength, int pointLightLength, int spotLightLength)
    {
        Span<int> sizes = stackalloc int[3];
        int index = 0;
        if (dirLightLength > 0)
        {
            sizes[index] = sizeof(DirLight) * dirLightLength;
            index++;
        }
        if (pointLightLength > 0)
        {
            sizes[index] = sizeof(PointLight) * pointLightLength;
            index++;
        }
        if (spotLightLength > 0)
        {
            sizes[index] = sizeof(SpotLight) * spotLightLength;
            index++;
        }

        var sizeArray = sizes.Slice(0, index).ToArray();

        LightSourceUniformBlock = deviceApi.CreateUniformBlockBuffer(sizeArray, BufferHint.Dynamic);

        _lastDirLightsLength = dirLightLength;
        _lastPointLightsLength = pointLightLength;
        _lastSpotLightsLength = spotLightLength;

        NumberOfLightChanged = true;
    }

    public void AfterUpdate() 
    {
        NumberOfLightChanged = false;
    }

    public void Dispose()
    {
        LightSourceUniformBlock?.Dispose();
    }
}

[StructLayout(LayoutKind.Sequential)]
internal struct SpotLight
{
    public Vector3 Position;
    float padding;
    public Vector3 Direction;
    float padding1;
    public Vector3 Color;
    public float CutOff;
    public float Intensity;
}

[StructLayout(LayoutKind.Sequential)]
internal struct PointLight
{
    public Vector3 Position;
    float padding;
    public Vector3 Color;
    public float Intensity;
}

[StructLayout(LayoutKind.Sequential)]
internal struct DirLight
{
    public Vector3 Direction;
    float padding;
    public Vector3 Color;
    public float Intensity;
}