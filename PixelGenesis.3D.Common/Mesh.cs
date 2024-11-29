using CommunityToolkit.HighPerformance;
using PixelGenesis.ECS.AssetManagement;
using System.Numerics;

namespace PixelGenesis._3D.Common;

public interface IMesh : IAsset
{    
    bool IsDirty { get; set; }
    ReadOnlyMemory<Vector3> Normals { get; }
    ReadOnlyMemory<Vector3> Vertices { get; }
    ReadOnlyMemory<uint> Triangles { get; }
    ReadOnlyMemory<Vector4> Colors { get; }
    ReadOnlyMemory<Vector4> Tangents { get; }
    ReadOnlyMemory<Vector2> UV1 { get; }
    ReadOnlyMemory<Vector2> UV2 { get; }
    ReadOnlyMemory<Vector2> UV3 { get; }
    ReadOnlyMemory<Vector2> UV4 { get; }
    ReadOnlyMemory<Vector2> UV5 { get; }
    ReadOnlyMemory<Vector2> UV6 { get; }
    ReadOnlyMemory<Vector2> UV7 { get; }
    ReadOnlyMemory<Vector2> UV8 { get; }

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
        WriteMemory(bw, mesh.UV3);
        WriteMemory(bw, mesh.UV4);
        WriteMemory(bw, mesh.UV5);
        WriteMemory(bw, mesh.UV6);
        WriteMemory(bw, mesh.UV7);
        WriteMemory(bw, mesh.UV8);
    }

    private static void WriteMemory<T>(BinaryWriter bw, ReadOnlyMemory<T> memory) where T : unmanaged
    {
        var byteSpan = memory.Span.AsBytes();
        bw.Write(byteSpan.Length);
        bw.Write(byteSpan);
    }

    IMesh Clone();   
}

public sealed class Mesh : IMesh
{
    public Guid Id { get; }
    public string Name { get; }
    public bool IsDirty { get; set; }    
    public ReadOnlyMemory<Vector3> Normals { get; }

    public ReadOnlyMemory<Vector3> Vertices { get; }

    public ReadOnlyMemory<uint> Triangles { get; }

    public ReadOnlyMemory<Vector4> Colors { get; }

    public ReadOnlyMemory<Vector4> Tangents { get; }

    public ReadOnlyMemory<Vector2> UV1 { get; }

    public ReadOnlyMemory<Vector2> UV2 { get; }
        
    public ReadOnlyMemory<Vector2> UV3 { get; }

    public ReadOnlyMemory<Vector2> UV4 { get; }

    public ReadOnlyMemory<Vector2> UV5 { get; }

    public ReadOnlyMemory<Vector2> UV6 { get; }

    public ReadOnlyMemory<Vector2> UV7 { get; }

    public ReadOnlyMemory<Vector2> UV8 { get; }

    private Mesh(Guid id, Stream stream, string? name = default)
    {
        Id = id;
        Name = name ?? $"{id}.pgmesh";

        using var br = new BinaryReader(stream);
        
        Normals = ReadMemory<Vector3>(br);
        Vertices = ReadMemory<Vector3>(br);
        Triangles = ReadMemory<uint>(br);
        Colors = ReadMemory<Vector4>(br);
        Tangents = ReadMemory<Vector4>(br);
        UV1 = ReadMemory<Vector2>(br);
        UV2 = ReadMemory<Vector2>(br);
        UV3 = ReadMemory<Vector2>(br);
        UV4 = ReadMemory<Vector2>(br);
        UV5 = ReadMemory<Vector2>(br);
        UV6 = ReadMemory<Vector2>(br);
        UV7 = ReadMemory<Vector2>(br);
        UV8 = ReadMemory<Vector2>(br);
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

    public void WriteToStream(IAssetManager assetManager, Stream stream)
    {
        IMesh.WriteMeshToStream(stream, this);
    }

    public class Factory : IReadAssetFactory
    {
        public IAsset ReadAsset(Guid id, IAssetManager assetManager, Stream stream)
        {
            return new Mesh(id, stream);
        }
    }
}

public sealed class MutableMesh : IMesh
{
    public Guid Id { get; }
    public string Name { get; }

    public MutableMesh(Guid id, string? name = default)
    {
        Id = id;
        Name = name ?? $"{id}.pgmesh";
    }

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

    public Memory<Vector2> MutableUV3;
    public ReadOnlyMemory<Vector2> UV3 => MutableUV3;

    public Memory<Vector2> MutableUV4;
    public ReadOnlyMemory<Vector2> UV4 => MutableUV4;

    public Memory<Vector2> MutableUV5;
    public ReadOnlyMemory<Vector2> UV5 => MutableUV5;

    public Memory<Vector2> MutableUV6;
    public ReadOnlyMemory<Vector2> UV6 => MutableUV6;

    public Memory<Vector2> MutableUV7;
    public ReadOnlyMemory<Vector2> UV7 => MutableUV7;

    public Memory<Vector2> MutableUV8;
    public ReadOnlyMemory<Vector2> UV8 => MutableUV8;

    public IMesh Clone()
    {
        var result = new MutableMesh(Guid.NewGuid());
        result.MutableNormals = MutableNormals.ToArray();
        result.MutableVertices = MutableVertices.ToArray();
        result.MutableTriangles = MutableTriangles.ToArray();
        result.MutableColors = MutableColors.ToArray();
        result.MutableTangents = MutableTangents.ToArray();
        result.MutableUV1 = MutableUV1.ToArray();
        result.MutableUV2 = MutableUV2.ToArray();
        result.MutableUV3 = MutableUV3.ToArray();
        result.MutableUV4 = MutableUV4.ToArray();
        result.MutableUV5 = MutableUV5.ToArray();
        result.MutableUV6 = MutableUV6.ToArray();
        result.MutableUV7 = MutableUV7.ToArray();
        result.MutableUV8 = MutableUV8.ToArray();


        return result;
    }

    public void WriteToStream(IAssetManager assetManager, Stream stream)
    {
        IMesh.WriteMeshToStream(stream, this);
    }
}
