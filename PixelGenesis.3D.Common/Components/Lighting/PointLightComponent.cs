using PixelGenesis.ECS;
using PixelGenesis.ECS.Components;
using System.Numerics;

namespace PixelGenesis._3D.Common.Components.Lighting;

public sealed partial class PointLightComponent(Transform3DComponent transform) : Component
{
    public Vector3 Color;
    public float Intensity;

    public Transform3DComponent Transform => transform;
}
