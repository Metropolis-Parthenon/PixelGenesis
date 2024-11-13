using CommunityToolkit.HighPerformance;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using PixelGenesis._3D.Common;
using PixelGenesis._3D.Renderer.DeviceApi.Abstractions;
using PixelGenesis.Editor.Core;
using System.Numerics;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PixelGenesis.Editor.BuiltIn.AssetEditors.Shader;

internal class ShaderRenderer : IDisposable
{
    PGGLSLShaderSource.CompilationResult? _shader;
    Dictionary<int, object[]> _layoutValues = new Dictionary<int, object[]>();

    IDeviceApi _deviceApi;

    static IMesh Mesh = SphereMesh.CreateSphereMesh(0.25f, 30f, 30f);

    DrawContext drawContext = new DrawContext();
    IShaderProgram shaderProgram;
    IVertexBuffer vertexBuffer;
    IIndexBuffer<uint> indexBuffer;
    IUniformBlockBuffer projectionUniformBlock;
    Matrix4x4 ProjectionMatrix;
    VertexBufferLayout vertexBufferLayout = new VertexBufferLayout();
    IFrameBuffer frameBuffer;

    List<IUniformBlockBuffer> shaderUniformsValues = new List<IUniformBlockBuffer>();

    IDisposable WindowResizeSubscription;

    public unsafe ShaderRenderer(PGGLSLShaderSource source, IDeviceApi deviceApi, ICommandDispatcher commandDispatcher)
    {
        _deviceApi = deviceApi;
        if(source.ShaderDTO is null)
        {
            _shader = null;
            return;
        }

        frameBuffer = deviceApi.CreateFrameBuffer(1920, 1080);

        WindowResizeSubscription = commandDispatcher.Commands.Where(x => x is EditorWindowResized).Cast<EditorWindowResized>().Subscribe(x =>
        {
            frameBuffer.Rescale(x.Width, x.Height);
        });

        _shader = source.CompiledShader();

        if(_shader.Shader is not null)
        {
            shaderProgram = _deviceApi.CreateShaderProgram(
                _shader.Shader.Vertex,
                _shader.Shader.Fragment,
                _shader.Shader.Tessellation,
                _shader.Shader.Geometry
            );
        }
        
        _layoutValues = CreateLayoutValues(source.ShaderDTO);

        foreach (var block in source.ShaderDTO.Blocks)
        {
            var sizes = block.Parameters.Select(x => x.Type switch
            {
                _3D.Common.Type.Float => sizeof(float),
                _3D.Common.Type.Float2 => sizeof(Vector2),
                _3D.Common.Type.Float3 => sizeof(Vector3),
                _3D.Common.Type.Float4 => sizeof(Vector4),
                _3D.Common.Type.Mat4 => sizeof(Matrix4x4),
                _3D.Common.Type.Color4 => sizeof(Vector4),
                _ => 0
            }).ToArray();
            var uniform = _deviceApi.CreateUniformBlockBuffer(sizes, BufferHint.Dynamic);
            shaderUniformsValues.Add(uniform);
        }

        projectionUniformBlock = deviceApi.CreateUniformBlockBuffer<Matrix4x4>(BufferHint.Static);
        
        vertexBuffer
            = deviceApi.CreateVertexBuffer(
                Mesh.Vertices.Length * sizeof(Vector3) 
                 + Mesh.Normals.Length * sizeof(Vector3)
                 + Mesh.UV1.Length * sizeof(Vector2), 
                BufferHint.Static);
        vertexBufferLayout.PushFloat(3, false);
        vertexBufferLayout.PushFloat(3, false);
        vertexBufferLayout.PushFloat(2, false);

        var vertexBufferData = new float[
                (Mesh.Vertices.Length * sizeof(Vector3) 
                 + Mesh.Normals.Length * sizeof(Vector3)
                 + Mesh.UV1.Length * sizeof(Vector2))/sizeof(float)];

        int index = 0;       
        for(int i = 0; i < vertexBufferData.Length; i+=8)
        {
            var vertex = Mesh.Vertices.Span[index];
            var normal = Mesh.Normals.Span[index];
            var uv = Mesh.UV1.Span[index];
            vertexBufferData[i] = vertex.X;
            vertexBufferData[i + 1] = vertex.Y;
            vertexBufferData[i + 2] = vertex.Z;
            vertexBufferData[i + 3] = normal.X;
            vertexBufferData[i + 4] = normal.Y;
            vertexBufferData[i + 5] = normal.Z;
            vertexBufferData[i + 6] = uv.X;
            vertexBufferData[i + 7] = uv.Y;
            index++;
        }

        vertexBuffer.SetData(0, vertexBufferData.AsSpan().AsBytes());

        indexBuffer 
            = deviceApi.CreateIndexBuffer<uint>(
                Mesh.Triangles.Length,
                BufferHint.Static);

        indexBuffer.SetData(0, Mesh.Triangles.Span);

        drawContext.VertexBuffer = vertexBuffer;
        drawContext.IndexBuffer = indexBuffer;
        drawContext.Layout = vertexBufferLayout;
    }

    public unsafe void OnSourceChanged(PGGLSLShaderSource changed)
    {
        if(changed.ShaderDTO is null)
        {
            return;
        }
        _shader = changed.CompiledShader();
        UpdateLayout(_layoutValues, changed.ShaderDTO);

        shaderProgram?.Dispose();
        shaderProgram = null;
        if (_shader.Shader is not null)
        {
            shaderProgram = _deviceApi.CreateShaderProgram(
                _shader.Shader.Vertex,
                _shader.Shader.Fragment,
                _shader.Shader.Tessellation,
                _shader.Shader.Geometry
            );
        }

        var uniformsSpan = CollectionsMarshal.AsSpan(shaderUniformsValues);

        foreach(var item in uniformsSpan)
        {
            item.Dispose();
        }

        shaderUniformsValues.Clear();

        foreach(var block in changed.ShaderDTO.Blocks)
        {
            var sizes = block.Parameters.Select(x => x.Type switch
            {
                _3D.Common.Type.Float => sizeof(float),
                _3D.Common.Type.Float2 => sizeof(Vector2),
                _3D.Common.Type.Float3 => sizeof(Vector3),
                _3D.Common.Type.Float4 => sizeof(Vector4),
                _3D.Common.Type.Mat4 => sizeof(Matrix4x4),
                _3D.Common.Type.Color4 => sizeof(Vector4),
                _ => 0
            }).ToArray();
            var uniform = _deviceApi.CreateUniformBlockBuffer(sizes, BufferHint.Dynamic);
            shaderUniformsValues.Add(uniform);
        }
    }

    ref T GetValueRef<T>(int binding, int index) where T : struct
    {
        if(!_layoutValues.TryGetValue(binding, out var values))
        {
            throw new InvalidOperationException("Binding not found");
        }

        if(index >= values.Length)
        {
            throw new IndexOutOfRangeException("Index out of range");
        }
                                
        return ref Unsafe.Unbox<T>(values[index]);
    }
    
    public void OnGui()
    {
        OnShaderPropertiesGui();

        OnRenderPreview();
    }

    List<float> Vertices = new List<float>();

    void OnRenderPreview()
    {
        ProjectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(OpenTK.Mathematics.MathHelper.DegreesToRadians(45), 1920 / 1080, 0.1f, 100);
        if (_shader?.Shader is null)
        {
            return;
        }

        frameBuffer.Bind();
        
        if (shaderProgram is not null)
        {
            SetUniformBlockValues();
            projectionUniformBlock.SetData(ProjectionMatrix, 0);
            shaderProgram.SetUniformBlock(1, projectionUniformBlock);
            drawContext.ShaderProgram = shaderProgram;
            drawContext.Lenght = Mesh.Triangles.Length;
            _deviceApi.DrawTriangles(drawContext);
        }

        frameBuffer.Unbind();

        
        ImGui.Image(frameBuffer.GetTexture().Id, new Vector2(600, 600));
    }

    void SetUniformBlockValues()
    {
        var uniformsSpan = CollectionsMarshal.AsSpan(shaderUniformsValues);

        for (var i = 0; i < uniformsSpan.Length; i++)
        {
            var uniform = uniformsSpan[i];
            var values = _layoutValues[i];
            for (var j = 0; j < values.Length; j++)
            {
                var value = values[j];
                switch (value)
                {
                    case float floatValue:
                        uniform.SetData(floatValue, j);
                        break;
                    case Vector2 vector2Value:
                        uniform.SetData(vector2Value, j);
                        break;
                    case Vector3 vector3Value:
                        uniform.SetData(vector3Value, j);
                        break;
                    case Vector4 vector4Value:
                        uniform.SetData(vector4Value, j);
                        break;
                    case Matrix4x4 matrix4x4Value:
                        uniform.SetData(matrix4x4Value, j);
                        break;
                }
            }
            shaderProgram.SetUniformBlock(i, uniform);
        }
    }

    void OnShaderPropertiesGui()
    {
        if (_shader is null)
        {
            ImGui.Text("Invalid File");
            return;
        }

        if (_shader.Shader is null)
        {
            ImGui.Text(_shader.Error);
            return;
        }

        _shader.Shader.Layout.Blocks.ForEach(block =>
        {
            for (var i = 0; i < block.Parameters.Count; i++)
            {
                var parameter = block.Parameters[i];

                if (parameter is null) continue;

                switch (parameter.Type)
                {
                    case _3D.Common.Type.Float:
                        ImGui.SliderFloat(parameter.Name, ref GetValueRef<float>(block.Binding, i), parameter?.Range?.Min ?? 0f, parameter?.Range?.Max ?? 1f);
                        break;
                    case _3D.Common.Type.Float2:
                        ImGui.SliderFloat2(parameter.Name, ref GetValueRef<Vector2>(block.Binding, i), parameter?.Range?.Min ?? 0f, parameter?.Range?.Max ?? 1f);
                        break;
                    case _3D.Common.Type.Float3:
                        ImGui.SliderFloat3(parameter.Name, ref GetValueRef<Vector3>(block.Binding, i), parameter?.Range?.Min ?? 0f, parameter?.Range?.Max ?? 1f);
                        break;
                    case _3D.Common.Type.Float4:
                        ImGui.SliderFloat4(parameter.Name, ref GetValueRef<Vector4>(block.Binding, i), parameter?.Range?.Min ?? 0f, parameter?.Range?.Max ?? 1f);
                        break;
                    case _3D.Common.Type.Mat4:
                        ImGui.Text("Mat4 not supported");
                        break;
                    case _3D.Common.Type.Color4:
                        ImGui.ColorEdit4(parameter.Name, ref GetValueRef<Vector4>(block.Binding, i));
                        break;
                }
            }
        });
    }


    static void UpdateLayout(Dictionary<int, object[]> layoutValues, ShaderSourceDTO layout)
    {
        layout.Blocks.ForEach(block =>
        {
            if(!layoutValues.TryGetValue(block.Binding, out var values))
            {
                values = new object[block.Parameters.Count];
            }

            if(values.Length != block.Parameters.Count)
            {
                Array.Resize(ref values, block.Parameters.Count);
            }

            for (var i = 0; i < block.Parameters.Count; i++)
            {
                var parameter = block.Parameters[i];
                switch (parameter.Type)
                {
                    case _3D.Common.Type.Float:
                        values[i] = values[i] is not null && values[i] is float ? values[i] : 0f;
                        break;
                    case _3D.Common.Type.Float2:
                        values[i] = values[i] is not null && values[i] is Vector2 ? values[i] : new Vector2();
                        break;
                    case _3D.Common.Type.Float3:
                        values[i] = values[i] is not null && values[i] is Vector3 ? values[i] : new Vector3();
                        break;
                    case _3D.Common.Type.Float4:
                        values[i] = values[i] is not null && values[i] is Vector4 ? values[i] : new Vector4();
                        break;
                    case _3D.Common.Type.Mat4:
                        values[i] = values[i] is not null && values[i] is Matrix4x4 ? values[i] : new Matrix4x4();
                        break;
                    case _3D.Common.Type.Color4:
                        values[i] = values[i] is not null && values[i] is Vector4 ? values[i] : new Vector4();
                        break;
                }
            }
                        
            layoutValues[block.Binding] = values;
        });
    }

    static Dictionary<int, object[]> CreateLayoutValues(ShaderSourceDTO layout)
    {
        var result = new Dictionary<int, object[]>();

        layout.Blocks.ForEach(block =>
        {
            var values = new object[block.Parameters.Count];

            for (var i = 0; i < block.Parameters.Count; i++)
            {
                var parameter = block.Parameters[i];
                if(parameter is null) continue;
                switch (parameter.Type)
                {
                    case _3D.Common.Type.Float:
                        values[i] = 0f;
                        break;
                    case _3D.Common.Type.Float2:
                        values[i] = new Vector2();
                        break;
                    case _3D.Common.Type.Float3:
                        values[i] = new Vector3();
                        break;
                    case _3D.Common.Type.Float4:
                        values[i] = new Vector4();
                        break;
                    case _3D.Common.Type.Mat4:
                        values[i] = new Matrix4x4();
                        break;
                    case _3D.Common.Type.Color4:
                        values[i] = new Vector4();
                        break;
                }
            }
            result[block.Binding] = values;
        });

        return result;
    }

    public void Dispose()
    {
        WindowResizeSubscription.Dispose();
        shaderProgram?.Dispose();
        vertexBuffer?.Dispose();
        indexBuffer?.Dispose();
        projectionUniformBlock?.Dispose();
        foreach(var item in shaderUniformsValues)
        {
            item.Dispose();
        }
    }
}
