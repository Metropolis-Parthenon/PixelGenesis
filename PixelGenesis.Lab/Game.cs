using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using CommunityToolkit.HighPerformance;
using System.Numerics;
using ImGuiNET;
using PixelGenesis.Lab.Tests;

namespace PixelGenesis.Lab;

internal class Game : GameWindow
{
    public Game(int width, int height, string title) : base(GameWindowSettings.Default, new NativeWindowSettings() { ClientSize = (width, height), Title = title }) { }

    ImGuiController _controller;

    Dictionary<string, Func<ITest>> Tests = new Dictionary<string, Func<ITest>>()
    {
        ["Texture"] = () => new TextureTest(),
        ["Clear Color"] = () => new ClearColorTest()
    };

    Stack<string> History = new Stack<string>();

    ITest? Test;
    
    protected override void OnLoad()
    {
        base.OnLoad();

        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);        

        _controller = new ImGuiController(ClientSize.X, ClientSize.Y);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        _controller.Update(this, (float)args.Time);
        OnGui();

        GL.Clear(ClearBufferMask.ColorBufferBit);
        Renderer.GLClearError();

        Test?.OnUpdate((float)args.Time);
        Test?.OnRender();


        _controller.Render();
        Renderer.GLCheckError();
        SwapBuffers();
    }

    void OnGui()
    {
        ImGui.Begin("Learning OpenGL");
        if(ImGui.BeginMainMenuBar())
        {
            foreach(var key in Tests.Keys)
            {
                if(ImGui.MenuItem(key))
                {
                    LoadTest(key);
                }
            }

            if(ImGui.Button("Back"))
            {
                PreviousTest();
            }
        }
        ImGui.EndMainMenuBar();

        Test?.OnGui();

        ImGui.End();
    }

    void PreviousTest()
    {
        if (History.Count <= 1) return;
        Test?.Dispose();        
        History.Pop();        
        var previous = History.Peek();
        Test = Tests[previous]();
    }

    void LoadTest(string name)
    {
        Test?.Dispose();
        Test = Tests[name]();
        History.Push(name);
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
        base.OnUnload();
    }

}
