using PixelGenesis.ECS;
using System.Numerics;

namespace PixelGenesis._3D.Common.Components;

public sealed partial class Transform3DComponent : Component
{
    public Vector3 Position = Vector3.Zero;
    public Quaternion Rotation = Quaternion.Zero;
    public Vector3 Scale = Vector3.One;
        
    public Matrix4x4 GetModelMatrix()
    {
        var result = Matrix4x4.Identity;
        result.Translation = Position;
        
        result *= Matrix4x4.CreateScale(Scale);
        result *= Matrix4x4.CreateFromQuaternion(Rotation);

        return result;        
    }

}
