using CommunityToolkit.HighPerformance;
using PixelGenesis.GameLogic;
using System.Numerics;

namespace PixelGenesis._3D.Common;

public interface IMesh : IStateObject
{
    ReadOnlyMemory<Vector3> Normals { get; }
    ReadOnlyMemory<Vector3> Vertices { get; }
    ReadOnlyMemory<int> Triangles { get; }
    ReadOnlyMemory<Vector4> Colors { get; }
    ReadOnlyMemory<Vector4> Tangents { get; }
    ReadOnlyMemory<Vector2> UV1 { get; }
    ReadOnlyMemory<Vector2> UV2 { get; }

    void WriteToStream(Stream stream)
    {
        using var bw = new BinaryWriter(stream);

        WriteMemory(bw, Normals);
        WriteMemory(bw, Vertices);
        WriteMemory(bw, Triangles);
        WriteMemory(bw, Colors);
        WriteMemory(bw, Tangents);
        WriteMemory(bw, UV1);
        WriteMemory(bw, UV2);
    }

    private static void WriteMemory<T>(BinaryWriter bw, ReadOnlyMemory<T> memory) where T : unmanaged
    {
        var byteSpan = memory.Span.AsBytes();
        bw.Write(byteSpan.Length);
        bw.Write(byteSpan);
    }
}


public sealed class Mesh : IMesh
{
    public ReadOnlyMemory<Vector3> Normals { get; }

    public ReadOnlyMemory<Vector3> Vertices { get; }

    public ReadOnlyMemory<int> Triangles { get; }

    public ReadOnlyMemory<Vector4> Colors { get; }

    public ReadOnlyMemory<Vector4> Tangents { get; }

    public ReadOnlyMemory<Vector2> UV1 { get; }

    public ReadOnlyMemory<Vector2> UV2 { get; }

    public Mesh(IMesh mesh)
    {
        Normals = mesh.Normals.ToArray();
        Vertices = mesh.Vertices.ToArray();
        Triangles = mesh.Triangles.ToArray();
        Colors = mesh.Colors.ToArray();
        Tangents = mesh.Tangents.ToArray();
        UV1 = mesh.UV1.ToArray();
        UV2 = mesh.UV2.ToArray();
    }

    public Mesh(Stream stream)
    {
        using var br = new BinaryReader(stream);
        
        Normals = ReadMemory<Vector3>(br);
        Vertices = ReadMemory<Vector3>(br);
        Triangles = ReadMemory<int>(br);
        Colors = ReadMemory<Vector4>(br);
        Tangents = ReadMemory<Vector4>(br);
        UV1 = ReadMemory<Vector2>(br);
        UV2 = ReadMemory<Vector2>(br);
    }

    static ReadOnlyMemory<T> ReadMemory<T>(BinaryReader br) where T : unmanaged
    {
        var lenght = br.ReadInt32();
        var bytes = br.ReadBytes(lenght);

        var result = bytes.AsMemory().Cast<byte, T>();

        return result;
    }

    public IStateObject DeepClone()
    {
        return this;
    }

}

public sealed class MutableMesh : IMesh
{
    public Memory<Vector3> MutableNormals;
    public ReadOnlyMemory<Vector3> Normals => MutableNormals;


    public Memory<Vector3> MutableVertices;
    public ReadOnlyMemory<Vector3> Vertices => MutableVertices;

    public Memory<int> MutableTriangles;
    public ReadOnlyMemory<int> Triangles => MutableTriangles;

    public Memory<Vector4> MutableColors;
    public ReadOnlyMemory<Vector4> Colors => MutableColors;

    public Memory<Vector4> MutableTangents;
    public ReadOnlyMemory<Vector4> Tangents => MutableTangents;

    public Memory<Vector2> MutableUV1;
    public ReadOnlyMemory<Vector2> UV1 => MutableUV1;

    public Memory<Vector2> MutableUV2;
    public ReadOnlyMemory<Vector2> UV2 => MutableUV2;

    public IStateObject DeepClone()
    {
        var result = new MutableMesh();
        result.MutableNormals = MutableNormals.ToArray();
        result.MutableVertices = MutableVertices.ToArray();
        result.MutableTriangles = MutableTriangles.ToArray();
        result.MutableColors = MutableColors.ToArray();
        result.MutableTangents = MutableTangents.ToArray();
        result.MutableUV1 = MutableUV1.ToArray();
        result.MutableUV2 = MutableUV2.ToArray();

        return result;
    }
}
