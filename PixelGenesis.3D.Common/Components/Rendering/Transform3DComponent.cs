using PixelGenesis.ECS;
using System.Numerics;

namespace PixelGenesis._3D.Common.Components;

public sealed partial class Transform3DComponent : Component
{
    public Vector3 Position;
    public Vector3 Rotation;
    public Vector3 Scale;
}
