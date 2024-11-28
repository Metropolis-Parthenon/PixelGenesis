using System.Numerics;

namespace PixelGenesis._3D.Common.Geometry;

public static class MeshGenerator
{
    public static IMesh SkyboxCube = new MutableMesh(Guid.NewGuid())
    {
        MutableVertices = new Memory<Vector3>([
            new Vector3(-1.0f,  1.0f, -1.0f),
            new Vector3(-1.0f, -1.0f, -1.0f),
            new Vector3( 1.0f, -1.0f, -1.0f),
            new Vector3( 1.0f, -1.0f, -1.0f),
            new Vector3( 1.0f,  1.0f, -1.0f),
            new Vector3(-1.0f,  1.0f, -1.0f),
            
            new Vector3(-1.0f, -1.0f,  1.0f),
            new Vector3(-1.0f, -1.0f, -1.0f),
            new Vector3(-1.0f,  1.0f, -1.0f),
            new Vector3(-1.0f,  1.0f, -1.0f),
            new Vector3(-1.0f,  1.0f,  1.0f),
            new Vector3(-1.0f, -1.0f,  1.0f),
            
            new Vector3( 1.0f, -1.0f, -1.0f),
            new Vector3( 1.0f, -1.0f,  1.0f),
            new Vector3( 1.0f,  1.0f,  1.0f),
            new Vector3( 1.0f,  1.0f,  1.0f),
            new Vector3( 1.0f,  1.0f, -1.0f),
            new Vector3( 1.0f, -1.0f, -1.0f),
            
            new Vector3(-1.0f, -1.0f,  1.0f),
            new Vector3(-1.0f,  1.0f,  1.0f),
            new Vector3( 1.0f,  1.0f,  1.0f),
            new Vector3( 1.0f,  1.0f,  1.0f),
            new Vector3( 1.0f, -1.0f,  1.0f),
            new Vector3(-1.0f, -1.0f,  1.0f),
            
            new Vector3(-1.0f,  1.0f, -1.0f),
            new Vector3( 1.0f,  1.0f, -1.0f),
            new Vector3( 1.0f,  1.0f,  1.0f),
            new Vector3( 1.0f,  1.0f,  1.0f),
            new Vector3(-1.0f,  1.0f,  1.0f),
            new Vector3(-1.0f,  1.0f, -1.0f),
            
            new Vector3(-1.0f, -1.0f, -1.0f),
            new Vector3(-1.0f, -1.0f,  1.0f),
            new Vector3( 1.0f, -1.0f, -1.0f),
            new Vector3( 1.0f, -1.0f, -1.0f),
            new Vector3(-1.0f, -1.0f,  1.0f),
            new Vector3( 1.0f, -1.0f,  1.0f)
            ]),
        MutableTriangles = new Memory<uint>([
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 ,11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35
        ]),
    };

    public static IMesh CubeMesh = new MutableMesh(Guid.NewGuid())
    {
        MutableVertices = new Memory<Vector3>([
        new Vector3(-0.5f, -0.5f, -0.5f),
        new Vector3(0.5f, -0.5f, -0.5f),
        new Vector3(0.5f,  0.5f, -0.5f),
        new Vector3(0.5f,  0.5f, -0.5f),
        new Vector3(-0.5f,  0.5f, -0.5f),
        new Vector3(-0.5f, -0.5f, -0.5f),

        new Vector3(-0.5f, -0.5f,  0.5f),
        new Vector3(0.5f, -0.5f,  0.5f),
        new Vector3(0.5f,  0.5f,  0.5f),
        new Vector3(0.5f,  0.5f,  0.5f),
        new Vector3(-0.5f,  0.5f,  0.5f),
        new Vector3(-0.5f, -0.5f,  0.5f),

        new Vector3(-0.5f,  0.5f,  0.5f),
        new Vector3(-0.5f,  0.5f, -0.5f),
        new Vector3(-0.5f, -0.5f, -0.5f),
        new Vector3(-0.5f, -0.5f, -0.5f),
        new Vector3(-0.5f, -0.5f,  0.5f),
        new Vector3(-0.5f,  0.5f,  0.5f),

        new Vector3(0.5f,  0.5f,  0.5f),
        new Vector3(0.5f,  0.5f, -0.5f),
        new Vector3(0.5f, -0.5f, -0.5f),
        new Vector3(0.5f, -0.5f, -0.5f),
        new Vector3(0.5f, -0.5f,  0.5f),
        new Vector3(0.5f,  0.5f,  0.5f),

        new Vector3(-0.5f, -0.5f, -0.5f),
        new Vector3(0.5f, -0.5f, -0.5f),
        new Vector3(0.5f, -0.5f,  0.5f),
        new Vector3(0.5f, -0.5f,  0.5f),
        new Vector3(-0.5f, -0.5f,  0.5f),
        new Vector3(-0.5f, -0.5f, -0.5f),

        new Vector3(-0.5f,  0.5f, -0.5f),
        new Vector3(0.5f,  0.5f, -0.5f),
        new Vector3(0.5f,  0.5f,  0.5f),
        new Vector3(0.5f,  0.5f,  0.5f),
        new Vector3(-0.5f,  0.5f,  0.5f),
        new Vector3(-0.5f,  0.5f, -0.5f),
        ]),

        MutableNormals = new Memory<Vector3>([
            new Vector3(0.0f,  0.0f, -1.0f),
            new Vector3(0.0f,  0.0f, -1.0f),
            new Vector3(0.0f,  0.0f, -1.0f),
            new Vector3(0.0f,  0.0f, -1.0f),
            new Vector3(0.0f,  0.0f, -1.0f),
            new Vector3(0.0f,  0.0f, -1.0f),

            new Vector3(0.0f,  0.0f, 1.0f),
            new Vector3(0.0f,  0.0f, 1.0f),
            new Vector3(0.0f,  0.0f, 1.0f),
            new Vector3(0.0f,  0.0f, 1.0f),
            new Vector3(0.0f,  0.0f, 1.0f),
            new Vector3(0.0f,  0.0f, 1.0f),

            new Vector3(1.0f,  0.0f,  0.0f),
            new Vector3(1.0f,  0.0f,  0.0f),
            new Vector3(1.0f,  0.0f,  0.0f),
            new Vector3(1.0f,  0.0f,  0.0f),
            new Vector3(1.0f,  0.0f,  0.0f),
            new Vector3(1.0f,  0.0f,  0.0f),

            new Vector3(1.0f,  0.0f,  0.0f),
            new Vector3(1.0f,  0.0f,  0.0f),
            new Vector3(1.0f,  0.0f,  0.0f),
            new Vector3(1.0f,  0.0f,  0.0f),
            new Vector3(1.0f,  0.0f,  0.0f),
            new Vector3(1.0f,  0.0f,  0.0f),

            new Vector3(0.0f, -1.0f,  0.0f),
            new Vector3(0.0f, -1.0f,  0.0f),
            new Vector3(0.0f, -1.0f,  0.0f),
            new Vector3(0.0f, -1.0f,  0.0f),
            new Vector3(0.0f, -1.0f,  0.0f),
            new Vector3(0.0f, -1.0f,  0.0f),

            new Vector3(0.0f,  1.0f,  0.0f),
            new Vector3(0.0f,  1.0f,  0.0f),
            new Vector3(0.0f,  1.0f,  0.0f),
            new Vector3(0.0f,  1.0f,  0.0f),
            new Vector3(0.0f,  1.0f,  0.0f),
            new Vector3(0.0f,  1.0f,  0.0f)
        ]),

        MutableTriangles = new Memory<uint>([
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 ,11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35
        ]),
    };


    public static IMesh Tetrahedron()
    {
        float a = 1.0f / 3.0f;
        float b = MathF.Sqrt(8.0f / 9.0f);
        float c = MathF.Sqrt(2.0f / 9.0f);
        float d = MathF.Sqrt(2.0f / 3.0f);

        Vector3[] vertices = [
            new Vector3(0, 0, 1),
            new Vector3(-c, d, -a),
            new Vector3(-c, -d, -a),
            new Vector3(b, 0, -a)
            ];

        uint[] triangles = [
            0, 1, 2,
            0, 2, 3,
            0, 3, 1,
            3, 2, 1];

        return new MutableMesh(Guid.NewGuid())
        {
            MutableVertices = new Memory<Vector3>(vertices),
            MutableTriangles = new Memory<uint>(triangles)
        };
    }
}
