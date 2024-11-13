using CommunityToolkit.HighPerformance;
using PixelGenesis.ECS;
using System.Numerics;

namespace PixelGenesis._3D.Common;

public interface IMesh : IWritableAsset
{
    Guid Id { get; }
    bool IsDirty { get; set; }
    ReadOnlyMemory<Vector3> Normals { get; }
    ReadOnlyMemory<Vector3> Vertices { get; }
    ReadOnlyMemory<uint> Triangles { get; }
    ReadOnlyMemory<Vector4> Colors { get; }
    ReadOnlyMemory<Vector4> Tangents { get; }
    ReadOnlyMemory<Vector2> UV1 { get; }
    ReadOnlyMemory<Vector2> UV2 { get; }

    internal static void WriteMeshToStream(Stream stream, IMesh mesh)
    {
        using var bw = new BinaryWriter(stream);

        WriteMemory(bw, mesh.Normals);
        WriteMemory(bw, mesh.Vertices);
        WriteMemory(bw, mesh.Triangles);
        WriteMemory(bw, mesh.Colors);
        WriteMemory(bw, mesh.Tangents);
        WriteMemory(bw, mesh.UV1);
        WriteMemory(bw, mesh.UV2);
    }

    private static void WriteMemory<T>(BinaryWriter bw, ReadOnlyMemory<T> memory) where T : unmanaged
    {
        var byteSpan = memory.Span.AsBytes();
        bw.Write(byteSpan.Length);
        bw.Write(byteSpan);
    }

    IMesh Clone();   
}

[ReadableAsset<Mesh, MeshFactory>]
public sealed class Mesh : IMesh, IReadableAsset
{
    public bool IsDirty { get; set; }
    public Guid Id { get; } = Guid.NewGuid();

    public ReadOnlyMemory<Vector3> Normals { get; }

    public ReadOnlyMemory<Vector3> Vertices { get; }

    public ReadOnlyMemory<uint> Triangles { get; }

    public ReadOnlyMemory<Vector4> Colors { get; }

    public ReadOnlyMemory<Vector4> Tangents { get; }

    public ReadOnlyMemory<Vector2> UV1 { get; }

    public ReadOnlyMemory<Vector2> UV2 { get; }

    public string Reference { get; }

    private Mesh(string reference, Stream stream)
    {
        Reference = reference;

        using var br = new BinaryReader(stream);
        
        Normals = ReadMemory<Vector3>(br);
        Vertices = ReadMemory<Vector3>(br);
        Triangles = ReadMemory<uint>(br);
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

    public IMesh Clone()
    {
        return this;
    }

    public void WriteToStream(Stream stream)
    {
        IMesh.WriteMeshToStream(stream, this);
    }

    public class MeshFactory : IReadableAssetFactory<Mesh>
    {
        public Mesh ReadAsset(string reference, Stream stream)
        {
            return new Mesh(reference, stream);
        }
    }
}

public sealed class MutableMesh : IMesh
{
    public Guid Id { get; } = Guid.NewGuid();
    
    public bool IsDirty { get; set; }

    public Memory<Vector3> MutableNormals;
    public ReadOnlyMemory<Vector3> Normals => MutableNormals;


    public Memory<Vector3> MutableVertices;
    public ReadOnlyMemory<Vector3> Vertices => MutableVertices;

    public Memory<uint> MutableTriangles;
    public ReadOnlyMemory<uint> Triangles => MutableTriangles;

    public Memory<Vector4> MutableColors;
    public ReadOnlyMemory<Vector4> Colors => MutableColors;

    public Memory<Vector4> MutableTangents;
    public ReadOnlyMemory<Vector4> Tangents => MutableTangents;

    public Memory<Vector2> MutableUV1;
    public ReadOnlyMemory<Vector2> UV1 => MutableUV1;

    public Memory<Vector2> MutableUV2;
    public ReadOnlyMemory<Vector2> UV2 => MutableUV2;

    public IMesh Clone()
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

    public void WriteToStream(Stream stream)
    {
        IMesh.WriteMeshToStream(stream, this);
    }
}
