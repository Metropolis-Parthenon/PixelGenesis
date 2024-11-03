using PixelGenesis.ECS;
using System.Numerics;

namespace PixelGenesis._3D.Common.Components.Phyisics;

public sealed partial class CapsuleColliderComponent : Component
{
    public bool IsTrigger;
    public PhysicMaterial? Material;
    public float Radius;
    public float Height;
    public Vector3 Center;
    public Axis Direction;
}

public enum Axis
{
    X,
    Y,
    Z
}