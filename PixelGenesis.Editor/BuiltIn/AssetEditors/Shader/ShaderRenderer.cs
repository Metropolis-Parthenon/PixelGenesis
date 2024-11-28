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
using System;
using System.Numerics;
using PixelGenesis._3D.Renderer.DeviceApi.Abstractions;
using ImGuiNET;

public class ShaderRenderer : IDisposable
{
    private IDeviceApi _deviceApi;
    private IShaderProgram _shaderProgram;
    private IVertexBuffer _vertexBuffer;
    private IIndexBuffer<uint> _indexBuffer;

    public ShaderRenderer(PGGLSLShaderSource source, IDeviceApi deviceApi)
    {
        Console.WriteLine("Initializing ShaderRenderer...");
        _deviceApi = deviceApi;

        // Compile and initialize shader program
        InitializeShaderProgram(source.CompiledShader());

        // Initialize triangle data
        InitializeTriangleData();
    }

    private void InitializeShaderProgram(PGGLSLShaderSource.CompilationResult compilationResult)
    {
        if (compilationResult.Shader == null)
        {
            Console.WriteLine("Shader compilation failed: " + compilationResult.Error);
            throw new InvalidOperationException("Shader compilation failed: " + compilationResult.Error);
        }

        _shaderProgram = _deviceApi.CreateShaderProgram(
            compilationResult.Shader.Vertex,
            compilationResult.Shader.Fragment,
            compilationResult.Shader.Tessellation,
            compilationResult.Shader.Geometry
        );
        Console.WriteLine("Shader program created.");
    }

    private void InitializeTriangleData()
    {
        // Triangle vertices
        float[] vertices = {
            -0.5f, -0.5f, 0.0f, // Bottom left
             0.5f, -0.5f, 0.0f, // Bottom right
             0.0f,  0.5f, 0.0f  // Top
        };

        _vertexBuffer = _deviceApi.CreateVertexBuffer(vertices.Length * sizeof(float), BufferHint.Static);
        _vertexBuffer.SetData(0, vertices.AsSpan().AsBytes());

        // Triangle indices
        uint[] indices = { 0, 1, 2 };
        _indexBuffer = _deviceApi.CreateIndexBuffer<uint>(indices.Length, BufferHint.Static);
        _indexBuffer.SetData(0, indices.AsSpan());

        Console.WriteLine("Triangle data initialized.");
    }

    private void Render()
    {
        Console.WriteLine("Clearing screen...");
        GL.ClearColor(0.0f, 0.0f, 1.0f, 1.0f); // Set clear color to bright blue
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        Console.WriteLine("Binding shader program and buffers...");
        _shaderProgram.Bind();
        _vertexBuffer.Bind();
        _indexBuffer.Bind();

        var layout = new BufferLayout();
        layout.PushFloat(3, false);
        Console.WriteLine("Drawing triangle...");
        _deviceApi.DrawTriangles(new DrawContext
        {
            ShaderProgram = _shaderProgram,
            VertexBuffer = _vertexBuffer,
            IndexBuffer = _indexBuffer,
            Layout = layout,
            Offset = 0,
            Lenght = 3 // Draw one triangle
        });

        Console.WriteLine("Render complete.");
    }

    public void OnGui()
    {
        Render(); // Render directly to the screen
    }

    public void Dispose()
    {
        Console.WriteLine("Disposing ShaderRenderer resources...");
        _shaderProgram?.Dispose();
        _vertexBuffer?.Dispose();
        _indexBuffer?.Dispose();
    }
}
