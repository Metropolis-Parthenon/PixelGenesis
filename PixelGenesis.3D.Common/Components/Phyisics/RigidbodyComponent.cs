using PixelGenesis.ECS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelGenesis._3D.Common.Components.Phyisics;

public sealed partial class RigidbodyComponent : Component
{
    public float Mass;
    public float Drag;
    public float AngularDrag;
    public bool UseGravity;
    public bool IsKinematic;
#warning Finish RigidbodyComponent
}
