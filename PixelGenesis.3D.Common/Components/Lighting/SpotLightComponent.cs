using PixelGenesis.ECS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PixelGenesis._3D.Common.Components.Lighting;

public sealed partial class SpotLightComponent(Transform3DComponent transform) : Component
{
    public Vector3 Direction;
    public Vector3 Color;
    public float Intensity;
    public float CutOff;

    public Transform3DComponent Transform => transform;
}
