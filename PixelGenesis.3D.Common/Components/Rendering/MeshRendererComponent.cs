using PixelGenesis.ECS;

namespace PixelGenesis._3D.Common.Components;

public sealed partial class MeshRendererComponent : Component
{
    public IMesh? Mesh;
    public Material? Material;
}
