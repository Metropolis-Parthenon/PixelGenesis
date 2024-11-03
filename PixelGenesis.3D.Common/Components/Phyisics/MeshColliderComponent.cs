using PixelGenesis.ECS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelGenesis._3D.Common.Components.Phyisics;

public sealed partial class MeshColliderComponent : Component
{
    public bool IsTrigger;
    public PhysicMaterial? Material;
    public bool Convex;
    public IMesh? Mesh;
}
