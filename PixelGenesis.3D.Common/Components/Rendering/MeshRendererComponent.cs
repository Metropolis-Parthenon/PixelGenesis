using PixelGenesis.ECS;

namespace PixelGenesis._3D.Common.Components;

public sealed partial class MeshRendererComponent(Transform3DComponent transform) : Component
{
    public IMesh? Mesh;
    public Material? Material;

    public Transform3DComponent GetTransform() => transform;
}
