using PixelGenesis.ECS;
using System.Numerics;

namespace PixelGenesis._3D.Common.Components.Phyisics;

public sealed partial class BoxColliderComponent : Component
{
    public bool IsTrigger;
    public PhysicMaterial? Material;    
    public Vector3 Center;
    public Vector3 Size;
}
