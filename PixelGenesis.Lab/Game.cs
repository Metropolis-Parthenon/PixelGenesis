using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using CommunityToolkit.HighPerformance;
using System.Numerics;
using ImGuiNET;

namespace PixelGenesis.Lab;

internal class Game : GameWindow
{
    public Game(int width, int height, string title) : base(GameWindowSettings.Default, new NativeWindowSettings() { ClientSize = (width, height), Title = title }) { }

    Matrix4x4 ProjectionMatrix = Matrix4x4.CreateOrthographicOffCenter(-2.0f, 2.0f, -1.5f, 1.5f, -1.0f, 1.0f);
    Matrix4x4 ViewMatrix = Matrix4x4.CreateTranslation(-0.5f, 0.0f, 0.0f);
    
    Matrix4x4 ModelMatrix;

    Vector3 TranslationA = new Vector3(0.5f, 0.5f, 0.0f);
    Vector3 TranslationB = new Vector3(0.5f, 0.5f, 0.0f);

    ImGuiController _controller;

    Texture Texture;
    Renderer Renderer = new Renderer();
    Shader Shader;
    VertexArrayObject vao;
    VertexBuffer vb;
    IndexBuffer ib;

    int program;

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

    protected override void OnLoad()
    {
        base.OnLoad();

        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);        

        _controller = new ImGuiController(ClientSize.X, ClientSize.Y);

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
            Path.Combine(Directory.GetCurrentDirectory(), "Assets", "shaders", "vertex.shader"),
            Path.Combine(Directory.GetCurrentDirectory(), "Assets", "shaders", "fragment.shader"));

    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        _controller.Update(this, (float)args.Time);
        OnGui();

        GL.Clear(ClearBufferMask.ColorBufferBit);
        Renderer.GLClearError();

        Texture.Bind(0);

        Shader.Bind();
        Shader.SetUniformMat4f("u_MVP", ProjectionMatrix * ViewMatrix * Matrix4x4.CreateTranslation(TranslationA));
        Renderer.Draw(vao, ib, Shader);

        Shader.Bind();
        Shader.SetUniformMat4f("u_MVP", ProjectionMatrix * ViewMatrix * Matrix4x4.CreateTranslation(TranslationB));
        Renderer.Draw(vao, ib, Shader);

        _controller.Render();
        Renderer.GLCheckError();
        SwapBuffers();
    }

    void OnGui()
    {
        ImGui.Begin("Hello, world!");

        ImGui.SliderFloat3("TranslationA", ref TranslationA, -1.0f, 1.0f);
        ImGui.SliderFloat3("TranslationB", ref TranslationB, -1.0f, 1.0f);

        ImGui.End();
    }

    protected override void OnTextInput(TextInputEventArgs e)
    {
        base.OnTextInput(e);
        _controller.PressChar((char)e.Unicode);
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);
        _controller.MouseScroll(e.Offset);
    }

    protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
    {
        base.OnFramebufferResize(e);

        // update opengl viewport
        GL.Viewport(0, 0, e.Width, e.Height);
        _controller.WindowResized(ClientSize.X, ClientSize.Y);
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        if (KeyboardState.IsKeyDown(Keys.Escape))
        {
            Close();
        }
    }

    protected override void OnUnload()
    {
        vb.Dispose();
        ib.Dispose();
        base.OnUnload();
    }

}
