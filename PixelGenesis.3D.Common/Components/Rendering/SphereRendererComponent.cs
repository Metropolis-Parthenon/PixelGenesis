using PixelGenesis.ECS;
using System.Numerics;

namespace PixelGenesis._3D.Common.Components;

public sealed partial class SphereRendererComponent(MeshRendererComponent meshRendererComponent) : Component, IUpdate
{
    static IMesh SphereMesh = CreateSphereMesh(.5f, 30f, 30f);

    public void Update()
    {
        meshRendererComponent.Mesh = SphereMesh;
    }

    static IMesh CreateSphereMesh(float radius, float sectors, float stacks)
    {
        List<Vector3> norms = new List<Vector3>();
        List<Vector3> verts = new List<Vector3>();
        List<uint> inds = new List<uint>();
        List<Vector2> texCoords = new List<Vector2>();

        var sectorStep = 2 * MathF.PI / sectors;
        var stackStep = MathF.PI / stacks;


        for (var i = 0; i <= stacks; i++)
        {
            var stackAngle = MathF.PI / 2 - i * stackStep;

            var xy = radius * MathF.Cos(stackAngle);
            var z = radius * MathF.Sin(stackAngle);

            for (var j = 0; j <= sectors; j++)
            {
                var sectorAngle = j * sectorStep;

                var x = xy * MathF.Cos(sectorAngle);
                var y = xy * MathF.Sin(sectorAngle);
                var vrtx = new Vector3(x, y, z);
                verts.Add(vrtx);

                var nVrtx = Vector3.Normalize(vrtx);
                norms.Add(nVrtx);

                texCoords.Add(new Vector2(j / sectors, i / stacks));
            }
        }

        var lineIndices = new List<uint>();
        for (var i = 0; i < stacks; ++i)
        {
            var k1 = i * (sectors + 1);     // beginning of current stack
            var k2 = k1 + sectors + 1;      // beginning of next stack

            for (int j = 0; j < sectors; ++j, ++k1, ++k2)
            {
                // 2 triangles per sector excluding first and last stacks
                // k1 => k2 => k1+1
                if (i != 0)
                {
                    inds.Add((uint)k1);
                    inds.Add((uint)k2);
                    inds.Add((uint)(k1 + 1));
                }

                // k1+1 => k2 => k2+1
                if (i != (stacks - 1))
                {
                    inds.Add((uint)(k1 + 1));
                    inds.Add((uint)k2);
                    inds.Add((uint)(k2 + 1));
                }

                // store indices for lines
                // vertical lines for all stacks, k1 => k2
                lineIndices.Add((uint)k1);
                lineIndices.Add((uint)k2);
                if (i != 0)  // horizontal lines except 1st stack, k1 => k+1
                {
                    lineIndices.Add((uint)k1);
                    lineIndices.Add((uint)(k1 + 1));
                }
            }
        }

        return new MutableMesh(Guid.NewGuid())
        {
            MutableVertices = verts.ToArray(),
            MutableTriangles = inds.ToArray(),
            MutableNormals = norms.ToArray(),
            MutableUV1 = texCoords.ToArray()
        };
    }

}
