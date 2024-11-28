using PixelGenesis._3D.Common.Geometry;
using PixelGenesis.ECS;
using System.Numerics;

namespace PixelGenesis._3D.Common.Components;



public sealed partial class CubeRendererComponent(MeshRendererComponent meshRendererComponent) : Component, IUpdate
{
    public void Update()
    {
        meshRendererComponent.Mesh = MeshGenerator.CubeMesh;
    }
}
