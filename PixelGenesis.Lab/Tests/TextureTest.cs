using CommunityToolkit.HighPerformance;
using ImGuiNET;
using PixelGenesis.Lab.OpenGLAbstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PixelGenesis.Lab.Tests;

internal class TextureTest : ITest
{

    Matrix4x4 ProjectionMatrix = Matrix4x4.CreateOrthographicOffCenter(-2.0f, 2.0f, -1.5f, 1.5f, -1.0f, 1.0f);
    Matrix4x4 ViewMatrix = Matrix4x4.CreateTranslation(-0.5f, 0.0f, 0.0f);

    Matrix4x4 ModelMatrix;

    Vector3 TranslationA = new Vector3(0.5f, 0.5f, 0.0f);
    Vector3 TranslationB = new Vector3(0.5f, 0.5f, 0.0f);

    Texture Texture;
    Renderer Renderer = new Renderer();
    Shader Shader;
    VertexArrayObject vao;
    VertexBuffer vb;
    IndexBuffer ib;
    UniformBuffer Projection;

    float[] positions = [
        -0.5f, -0.5f, 0.0f, 0.0f,
        0.5f, -0.5f, 1.0f, 0.0f,
        0.5f, 0.5f, 1.0f, 1.0f,
        -0.5f, 0.5f, 0.0f, 1.0f
        ];

    uint[] indices = [
        0, 1, 2,
        2, 3, 0
    ];


    public TextureTest()
    {
        Texture = new Texture(Path.Combine(Directory.GetCurrentDirectory(), "Assets", "textures", "logo.png"));

        vao = new VertexArrayObject();

        // vertex buffer
        vb = new VertexBuffer(positions.AsMemory().Cast<float, byte>());
        var layout = new VertexBufferLayout();
        layout.PushFloat(2);
        layout.PushFloat(2);

        vao.AddBuffer(vb, layout);

        //index buffer
        ib = new IndexBuffer(indices.AsMemory());

        Shader = new Shader(
            Path.Combine(Directory.GetCurrentDirectory(), "Assets", "shaders", "shader.vert"),
            Path.Combine(Directory.GetCurrentDirectory(), "Assets", "shaders", "shader.frag"));

        Projection = UniformBuffer.Create<Matrix4x4>();        
    }

    public void OnUpdate(float deltaTime)
    {
        ModelMatrix = ProjectionMatrix * ViewMatrix * Matrix4x4.CreateTranslation(TranslationA);        
    }

    public void OnGui()
    {
        ImGui.SliderFloat3("Translation A", ref TranslationA, -1.0f, 1.0f);
    }

    public void OnRender()
    {
        Shader.Bind();
        Projection.Bind();
        Projection.SetData(ModelMatrix, 0);
        Texture.Bind(0);
        Shader.SetUniformBlock("Projection", Projection);        
        // Shader.SetUniformMat4f("projection.u_MVP", ModelMatrix);
        Renderer.Draw(vao, ib, Shader);
    }

    public void Dispose()
    {
        vb.Dispose();
        ib.Dispose();
        vao.Dispose();
        Shader.Dispose();
        Texture.Dispose();
    }
}
