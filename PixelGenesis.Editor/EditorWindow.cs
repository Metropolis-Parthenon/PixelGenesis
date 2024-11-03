using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using ImGuiNET;
using OpenTK.Mathematics;
using PixelGenesis.Editor.GUI;

namespace PixelGenesis.Editor;

internal class EditorWindow : GameWindow
{
    PixelGenesisEditor EditorGUI;

    public EditorWindow(int width, int height, string title, PixelGenesisEditor editorGui) : base(
        GameWindowSettings.Default,
        new NativeWindowSettings() { ClientSize = (width, height), Title = title, WindowBorder = WindowBorder.Resizable }) 
    { 
        EditorGUI = editorGui;
        
    }

    ImGuiController _controller;
    
    protected override void OnLoad()
    {
        base.OnLoad();
        
        _controller = new ImGuiController(ClientSize.X, ClientSize.Y);
        EditorGUI.OnGuiInit();
        _controller.RecreateFontDeviceTexture();

        GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        _controller.Update(this, (float)args.Time);

        GL.ClearColor(new Color4(0, 32, 48, 255));
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

        EditorGUI.OnGui();

        ImGui.End();

        _controller.Render();

        ImGuiController.CheckGLError("End of frame");

        SwapBuffers();
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

        // Tell ImGui of the new size
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
