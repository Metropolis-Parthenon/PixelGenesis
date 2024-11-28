using CommunityToolkit.HighPerformance;
using PixelGenesis._3D.Common;
using PixelGenesis._3D.Renderer.DeviceApi.Abstractions;
using System.Numerics;

namespace PixelGenesis._3D.Renderer.DeviceObjects;

internal class RendererDeviceMesh(IDeviceApi deviceApi, IMesh mesh) : IRendererDeviceObject
{    
    public IMesh Mesh => mesh;

    public bool IsDirty { get; private set; }

    IVertexBuffer vertexBuffer;
    BufferLayout vertexBufferLayout = new BufferLayout();

    IIndexBuffer indexBuffer;

    public IVertexBuffer VertexBuffer => vertexBuffer ?? throw new ArgumentNullException(nameof(vertexBuffer));
    public BufferLayout VertexBufferLayout => vertexBufferLayout ?? throw new ArgumentNullException(nameof(vertexBufferLayout));
    public IIndexBuffer IndexBuffer => indexBuffer ?? throw new ArgumentNullException(nameof(indexBuffer));

    public int PositionLayout { get; private set; } = -1;
    public int NormalLayout { get; private set; } = -1;
    public int TangentLayout { get; private set; } = -1;
    public int ColorLayout { get; private set; } = -1;
    public int UV1Layout { get; private set; } = -1;
    public int UV2Layout { get; private set; } = -1;
    public int UV3Layout { get; private set; } = -1;
    public int UV4Layout { get; private set; } = -1;
    public int UV5Layout { get; private set; } = -1;
    public int UV6Layout { get; private set; } = -1;
    public int UV7Layout { get; private set; } = -1;
    public int UV8Layout { get; private set; } = -1;
    public int ModelLayout { get; private set; } = -1;

    public void Initialize()
    {
        CreateDeviceMesh();
        mesh.IsDirty = false;
        IsDirty = false;
    }

    public void Update()
    {
        if (mesh.IsDirty)
        {
            vertexBuffer.Dispose();
            indexBuffer.Dispose();
            vertexBufferLayout.Clear();
            CreateDeviceMesh();
            mesh.IsDirty = false;
            IsDirty = true;
        }
    }

    unsafe void CreateDeviceMesh()
    {        
        Span<ReadOnlyMemory<byte>> data = new ReadOnlyMemory<byte>[10];
        Span<int> sizes = stackalloc int[10];

        int lenght = 0;

        if (mesh.Vertices.Length > 0)
        {
            data[lenght] = mesh.Vertices.AsBytes();
            sizes[lenght] = sizeof(Vector3);
            vertexBufferLayout.PushFloat(3, false);
            PositionLayout = lenght;
            lenght++;
        }
        else
        {
            PositionLayout = -1;
        }

        if (mesh.Normals.Length > 0)
        {
            data[lenght] = mesh.Normals.AsBytes();
            sizes[lenght] = sizeof(Vector3);
            vertexBufferLayout.PushFloat(3, false);
            NormalLayout = lenght;
            lenght++;
        }
        else
        {
            NormalLayout = -1;
        }

        if (mesh.Tangents.Length > 0)
        {
            data[lenght] = mesh.Tangents.AsBytes();
            sizes[lenght] = sizeof(Vector4);
            vertexBufferLayout.PushFloat(4, false);
            TangentLayout = lenght;
            lenght++;
        }
        else
        {
            TangentLayout = -1;
        }

        if (mesh.Colors.Length > 0)
        {
            data[lenght] = mesh.Colors.AsBytes();
            sizes[lenght] = sizeof(Vector4);
            vertexBufferLayout.PushFloat(4, false);
            ColorLayout = lenght;
            lenght++;
        }
        else
        {
            ColorLayout = -1;
        }

        if (mesh.UV1.Length > 0)
        {
            data[lenght] = mesh.UV1.AsBytes();
            sizes[lenght] = sizeof(Vector2);
            vertexBufferLayout.PushFloat(2, false);
            UV1Layout = lenght;
            lenght++;
        }
        else
        {
            UV1Layout = -1;
        }

        if (mesh.UV2.Length > 0)
        {
            data[lenght] = mesh.UV2.AsBytes();
            sizes[lenght] = sizeof(Vector2);
            vertexBufferLayout.PushFloat(2, false);
            UV2Layout = lenght;
            lenght++;
        }
        else
        {
            UV2Layout = -1;
        }

        if (mesh.UV3.Length > 0)
        {
            data[lenght] = mesh.UV3.AsBytes();
            sizes[lenght] = sizeof(Vector2);
            vertexBufferLayout.PushFloat(2, false);
            UV3Layout = lenght;
            lenght++;
        }
        else
        {
            UV3Layout = -1;
        }

        if (mesh.UV4.Length > 0)
        {
            data[lenght] = mesh.UV4.AsBytes();
            sizes[lenght] = sizeof(Vector2);
            vertexBufferLayout.PushFloat(2, false);
            UV4Layout = lenght;
            lenght++;
        }
        else
        {
            UV4Layout = -1;
        }

        if (mesh.UV5.Length > 0)
        {
            data[lenght] = mesh.UV5.AsBytes();
            sizes[lenght] = sizeof(Vector2);
            vertexBufferLayout.PushFloat(2, false);
            UV5Layout = lenght;
            lenght++;
        }
        else
        {
            UV5Layout = -1;
        }

        if (mesh.UV6.Length > 0)
        {
            data[lenght] = mesh.UV6.AsBytes();
            sizes[lenght] = sizeof(Vector2);
            vertexBufferLayout.PushFloat(2, false);
            UV6Layout = lenght;
            lenght++;
        }
        else
        {
            UV6Layout = -1;
        }

        if (mesh.UV7.Length > 0)
        {
            data[lenght] = mesh.UV7.AsBytes();
            sizes[lenght] = sizeof(Vector2);
            vertexBufferLayout.PushFloat(2, false);
            UV7Layout = lenght;
            lenght++;
        }
        else
        {
            UV7Layout = -1;
        }

        if (mesh.UV8.Length > 0)
        {
            data[lenght] = mesh.UV8.AsBytes();
            sizes[lenght] = sizeof(Vector2);
            vertexBufferLayout.PushFloat(2, false);
            UV8Layout = lenght;
            lenght++;
        }
        else
        {
            UV8Layout = -1;
        }

        ModelLayout = lenght;

        data = data.Slice(0, lenght);
        sizes = sizes.Slice(0, lenght);

        var combinedBuffer = Combine(data, sizes);

        vertexBuffer = deviceApi.CreateVertexBuffer(combinedBuffer, BufferHint.Static);

        indexBuffer = deviceApi.CreateIndexBuffer(mesh.Triangles, BufferHint.Static);
    }


    static Memory<byte> Combine(ReadOnlySpan<ReadOnlyMemory<byte>> data, ReadOnlySpan<int> sizes)
    {
        var totalLength = 0;
        for (var i = 0; i < data.Length; i++)
            totalLength += data[i].Length;

        var result = new byte[totalLength];

        Span<int> indexes = stackalloc int[data.Length];

        for (int i = 0; i < totalLength;)
        {
            for (var j = 0; j < sizes.Length; j++)
            {
                var size = sizes[j];
                var index = indexes[j];
                for (var k = 0; k < size; k++, i++)
                {
                    result[i] = data[j].Span[index + k];
                }
                indexes[j] = index + size;
            }
        }

        return result;
    }
    public void AfterUpdate()
    {
        IsDirty = false;
    }

    public void Dispose()
    {
        VertexBuffer?.Dispose();
        IndexBuffer?.Dispose();
    }
    
}
