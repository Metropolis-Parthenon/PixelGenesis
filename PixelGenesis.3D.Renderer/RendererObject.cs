using PixelGenesis._3D.Common;
using PixelGenesis._3D.Renderer.DeviceApi.Abstractions;

namespace PixelGenesis._3D.Renderer;

public class RendererObject(int id)
{
    public int Id => id;

    public IVertexBuffer VertexBuffer { get; set; }
    public VertexBufferLayout VertexBufferLayout { get; set; }
    public IIndexBuffer IndexBuffer { get; set; }
    public IInstanceBuffer InstanceBuffer { get; set; }
    public VertexBufferLayout InstanceBufferLayout { get; set; }
    public MaterialRendererObject Material { get; set; }
    public int Instances { get; set; }
}

public class MaterialRendererObject(int id)
{
    public int Id => id;
    public IShaderProgram ShaderProgram { get; set; }
    public SortedList<int, IUniformBlockBuffer> Uniforms { get; set; }
}