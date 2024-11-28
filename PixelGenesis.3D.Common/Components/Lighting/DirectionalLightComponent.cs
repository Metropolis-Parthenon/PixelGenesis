using PixelGenesis.ECS;
using System.Numerics;

namespace PixelGenesis._3D.Common.Components.Lighting;

public sealed partial class DirectionalLightComponent(Transform3DComponent transform) : Component
{
    public Vector3 Direction;
    public Vector3 Color;
    public float Intensity;

    public Transform3DComponent Transform => transform;
}
