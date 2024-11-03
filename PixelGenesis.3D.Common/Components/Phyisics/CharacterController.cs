using PixelGenesis.ECS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PixelGenesis._3D.Common.Components.Phyisics;

public sealed partial class CharacterController : Component
{    
    public float StepOffset;
    public float SkinWidth;
    public float SlopeLimit;
    public float MinMoveDistance;

    public Vector3 Center;
    public float Radius;
    public float Height;
}
