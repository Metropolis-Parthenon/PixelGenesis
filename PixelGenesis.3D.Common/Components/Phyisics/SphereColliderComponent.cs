using PixelGenesis.ECS.Components;
using System.Numerics;

namespace PixelGenesis._3D.Common.Components.Phyisics;

public sealed partial class SphereColliderComponent : Component
{
    public bool IsTrigger;
    public PhysicMaterial? Material;
    public Vector3 Center;
    public float Radius;
}
