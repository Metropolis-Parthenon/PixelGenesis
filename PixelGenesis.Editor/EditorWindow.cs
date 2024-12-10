using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using ImGuiNET;
using PixelGenesis.Editor.GUI;
using PixelGenesis._3D.Renderer.DeviceApi.Abstractions;
using PixelGenesis.Editor.Core;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using System.Numerics;
using System.Drawing;
using PixelGenesis.ECS.Systems;

namespace PixelGenesis.Editor;

public record EditorWindowResized(int Width, int Height);

internal class EditorWindow : GameWindow, IPGWindow, IWindowInputs, ITime
{
    PixelGenesisEditor EditorGUI;

    IDeviceApi _deviceApi;

    ICommandDispatcher _commandDispatcher;

    public EditorWindow(int width, int height, string title, IDeviceApi deviceApi, PixelGenesisEditor editorGui, ICommandDispatcher commandDispatcher) : base(
        GameWindowSettings.Default,
        new NativeWindowSettings() { 
            ClientSize = (width, height), 
            Title = title, 
            WindowBorder = WindowBorder.Resizable,
#if DEBUG
            Flags = ContextFlags.Debug
#endif
        }) 
    { 
        EditorGUI = editorGui;
        _deviceApi = deviceApi;
        _commandDispatcher = commandDispatcher;

        ViewportSizeSubject = new BehaviorSubject<Size>(new Size(FramebufferSize.X, FramebufferSize.Y));
    }

    ImGuiPGController _controller;

    Size IPGWindow.ViewportSize => ViewportSizeSubject.Value;
    public IObservable<Size> ViewportSizeObservable => ViewportSizeSubject.AsObservable();

    BehaviorSubject<Size> ViewportSizeSubject;

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
        DeltaTime = (float)args.Time;
        DeltaTimeDouble = args.Time;

        EditorGUI.BeforeGui();

        _controller.Update(this, (float)args.Time);

        EditorGUI.OnGui();

        ImGui.End();

        GL.Viewport(0,0,FramebufferSize.X, FramebufferSize.Y);
        GL.ClearColor(0, 32, 48, 255);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

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

        ViewportSizeSubject.OnNext(new Size(e.Width, e.Height));
        _commandDispatcher.Dispatch(new EditorWindowResized(e.Width, e.Height));

        // update opengl viewport
        //GL.Viewport(0, 0, e.Width, e.Height);

        // Tell ImGui of the new size
        _controller.WindowResized(ClientSize.X, ClientSize.Y);
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        FixedDeltaTime = (float)args.Time;
        FixedDeltaTimeDouble = (double)args.Time;

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

    Vector2 IWindowInputs.MousePosition => new Vector2(MouseState.Position.X, MouseState.Position.Y);

    public Vector2 ScrollPosition => new Vector2(MouseState.Scroll.X, MouseState.Scroll.Y);

    public float Time { get; private set; }

    public double TimeDouble { get; private set; }

    public float DeltaTime { get; private set; }

    public double DeltaTimeDouble { get; private set; }

    public float FixedDeltaTime { get; private set; }

    public double FixedDeltaTimeDouble { get; private set; }

    public float TimeScale { get; set; } = 1f;    

    public bool IsKeyboardKeyDown(PGInputKey key)
    {
        return KeyboardState.IsKeyDown((Keys)key);
    }

    public bool IsMouseButtomDown(PGMouseButton button)
    {
        return MouseState.IsButtonDown((MouseButton)button);
    }
}
