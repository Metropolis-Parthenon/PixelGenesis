using PixelGenesis.ECS;
using System.Numerics;

namespace PixelGenesis._3D.Common.Components;

public sealed partial class PerspectiveCameraComponent(Transform3DComponent transform3D) : Component
{
    public float FieldOfView = 45f;

    public float NearPlaneDistance = 0.1f;

    public float FarPlaneDistance = 1000f;

    public Matrix4x4 GetProjectionMatrix(float aspectRatio)
    {
        return Matrix4x4.CreatePerspectiveFieldOfView((MathF.PI / 180) * FieldOfView, aspectRatio, NearPlaneDistance, FarPlaneDistance);
    }

    public Matrix4x4 GetViewMatrix()
    {
        var position = transform3D.Position;
        var rotation = transform3D.Rotation;

        var forward = Vector3.Transform(Vector3.UnitZ, rotation);
        var up = Vector3.Transform(Vector3.UnitY, rotation);

        return Matrix4x4.CreateLookAt(position, position + forward, up);
    }

}
