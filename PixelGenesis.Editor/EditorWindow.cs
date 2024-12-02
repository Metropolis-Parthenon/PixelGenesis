using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using ImGuiNET;
using OpenTK.Mathematics;
using PixelGenesis.Editor.GUI;
using PixelGenesis._3D.Renderer.DeviceApi.Abstractions;
using PixelGenesis.Editor.Core;

namespace PixelGenesis.Editor;

public record EditorWindowResized(int Width, int Height);

internal class EditorWindow : GameWindow, IPGWindow
{
    PixelGenesisEditor EditorGUI;

    IDeviceApi _deviceApi;

    ICommandDispatcher _commandDispatcher;

    public EditorWindow(int width, int height, string title, IDeviceApi deviceApi, PixelGenesisEditor editorGui, ICommandDispatcher commandDispatcher) : base(
        GameWindowSettings.Default,
        new NativeWindowSettings() { ClientSize = (width, height), Title = title, WindowBorder = WindowBorder.Resizable }) 
    { 
        EditorGUI = editorGui;
        _deviceApi = deviceApi;
        _commandDispatcher = commandDispatcher;
    }

    ImGuiPGController _controller;

    public float Width => Size.X;

    public float Height => Size.Y;

    protected override void OnLoad()
    {
        base.OnLoad();

        var vao = GL.GenVertexArray();
        GL.BindVertexArray(vao);

        _controller = new ImGuiPGController(_deviceApi, ClientSize.X, ClientSize.Y);
        EditorGUI.OnGuiInit();
        _controller.RecreateFontDeviceTexture();

        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        EditorGUI.BeforeGui();

        _controller.Update(this, (float)args.Time);

        GL.ClearColor(new Color4(0, 32, 48, 255));
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
        
        EditorGUI.OnGui();

        ImGui.End();

        _controller.Render();
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

        _commandDispatcher.Dispatch(new EditorWindowResized(e.Width, e.Height));

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
        _deviceApi.Dispose();
        base.OnUnload();
    }

}
