using ImGuiNET;
using System.Runtime.CompilerServices;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using PixelGenesis._3D.Renderer.DeviceApi.Abstractions;
using System.Numerics;
using System.Drawing;

namespace PixelGenesis.Editor;

public class ImGuiPGController : IDisposable
{
    IDeviceApi _deviceApi;

    private bool _frameBegun;

    IVertexBuffer _vertexBuffer;
    private int _vertexBufferSize;
    IIndexBuffer<ushort> _indexArrayBuffer;
    private int _indexBufferSize;

    IUniformBlockBuffer _projectionuniformBlockBuffer;

    VertexBufferLayout _layout;
    DrawContext _drawContext = new DrawContext()
    {
        EnableBlend = true,
        BlendEquation = GPBlendEquation.Add,
        BlendSFactor = PGBlendingFactor.SrcAlpha,
        BlendDFactor = PGBlendingFactor.OneMinusSrcAlpha,
        EnableDepthTest = false,
        EnableCullFace = false,
        EnableScissorTest = true,
    };

    ITexture _fontTexture;

    IShaderProgram _shaderProgram;

    private int _windowWidth;
    private int _windowHeight;

    private System.Numerics.Vector2 _scaleFactor = System.Numerics.Vector2.One;

    private static bool KHRDebugAvailable = false;

    private int GLVersion;
    private bool CompatibilityProfile;

    /// <summary>
    /// Constructs a new ImGuiController.
    /// </summary>
    public ImGuiPGController(IDeviceApi deviceApi, int width, int height)
    {
        _deviceApi = deviceApi;

        _windowWidth = width;
        _windowHeight = height;

        IntPtr context = ImGui.CreateContext();
        ImGui.SetCurrentContext(context);
        var io = ImGui.GetIO();
        io.Fonts.AddFontDefault();

        io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;
        // Enable Docking
        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;

        CreateDeviceResources();

        SetPerFrameImGuiData(1f / 60f);

        ImGui.NewFrame();
        _frameBegun = true;
    }

    public void WindowResized(int width, int height)
    {
        _windowWidth = width;
        _windowHeight = height;
    }

    public void DestroyDeviceObjects()
    {
        Dispose();
    }

    public void CreateDeviceResources()
    {
        _vertexBufferSize = 10000;
        _indexBufferSize = 2000;

        _vertexBuffer = _deviceApi.CreateVertexBuffer(_vertexBufferSize, BufferHint.Dynamic);

        _indexArrayBuffer = _deviceApi.CreateIndexBuffer<ushort>(_indexBufferSize, BufferHint.Dynamic);
        _projectionuniformBlockBuffer = _deviceApi.CreateUniformBlockBuffer<Matrix4x4>(BufferHint.Dynamic);

        RecreateFontDeviceTexture();

        string VertexSource = @"#version 450

layout (binding=0) uniform Projection
{
    mat4 projection_matrix;
} projection;

layout(location = 0) in vec2 in_position;
layout(location = 1) in vec2 in_texCoord;
layout(location = 2) in vec4 in_color;

layout(location = 0) out vec4 color;
layout(location = 1) out vec2 texCoord;

void main()
{
    gl_Position = projection.projection_matrix * vec4(in_position, 0, 1);
    color = in_color;
    texCoord = in_texCoord;
}";
        string FragmentSource = @"#version 450

layout(binding = 0) uniform sampler2D in_fontTexture;

layout(location = 0) in vec4 color;
layout(location = 1) in vec2 texCoord;

layout(location = 0) out vec4 outputColor;

void main()
{
    outputColor = color * texture(in_fontTexture, texCoord);
}";

        var vertexBytecode = ShadersHelper.CompileGLSLSourceToSpirvBytecode(VertexSource, "vert");
        var fragmentBytecode = ShadersHelper.CompileGLSLSourceToSpirvBytecode(FragmentSource, "frag");

        _shaderProgram = _deviceApi.CreateShaderProgram(
            vertexBytecode,
            fragmentBytecode,
            ReadOnlyMemory<byte>.Empty,
            ReadOnlyMemory<byte>.Empty);

        _layout = new VertexBufferLayout();
        _layout.PushFloat(2, false);
        _layout.PushFloat(2, false);
        _layout.PushByte(4, true);
    }

    /// <summary>
    /// Recreates the device texture used to render text.
    /// </summary>
    public void RecreateFontDeviceTexture()
    {
        ImGuiIOPtr io = ImGui.GetIO();
        io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out int width, out int height, out int bytesPerPixel);

        int mips = (int)Math.Floor(Math.Log(Math.Max(width, height), 2));

        _fontTexture = _deviceApi.CreateTexture(
            width,
            height,
            new UnmanagedMemoryManager<byte>(pixels, width * height).Memory,
            PGPixelFormat.Bgra,
            PGInternalPixelFormat.Rgba8,
            PGPixelType.UnsignedByte);

        io.Fonts.SetTexID(_fontTexture.Id);

        io.Fonts.ClearTexData();
    }

    /// <summary>
    /// Renders the ImGui draw list data.
    /// </summary>
    public void Render()
    {
        if (_frameBegun)
        {
            _frameBegun = false;
            ImGui.Render();
            RenderImDrawData(ImGui.GetDrawData());
        }
    }

    /// <summary>
    /// Updates ImGui input and IO configuration state.
    /// </summary>
    public void Update(GameWindow wnd, float deltaSeconds)
    {
        if (_frameBegun)
        {
            ImGui.Render();
        }

        SetPerFrameImGuiData(deltaSeconds);
        UpdateImGuiInput(wnd);

        _frameBegun = true;
        ImGui.NewFrame();
    }

    /// <summary>
    /// Sets per-frame data based on the associated window.
    /// This is called by Update(float).
    /// </summary>
    private void SetPerFrameImGuiData(float deltaSeconds)
    {
        ImGuiIOPtr io = ImGui.GetIO();
        io.DisplaySize = new Vector2(
            _windowWidth / _scaleFactor.X,
            _windowHeight / _scaleFactor.Y);
        io.DisplayFramebufferScale = _scaleFactor;
        io.DeltaTime = deltaSeconds; // DeltaTime is in seconds.
    }

    readonly List<char> PressedChars = new List<char>();

    private void UpdateImGuiInput(GameWindow wnd)
    {
        ImGuiIOPtr io = ImGui.GetIO();

        MouseState MouseState = wnd.MouseState;
        KeyboardState KeyboardState = wnd.KeyboardState;

        io.MouseDown[0] = MouseState[MouseButton.Left];
        io.MouseDown[1] = MouseState[MouseButton.Right];
        io.MouseDown[2] = MouseState[MouseButton.Middle];
        io.MouseDown[3] = MouseState[MouseButton.Button4];
        io.MouseDown[4] = MouseState[MouseButton.Button5];

        var screenPoint = new Vector2((int)MouseState.X, (int)MouseState.Y);
        var point = screenPoint;//wnd.PointToClient(screenPoint);
        io.MousePos = screenPoint;

        foreach (Keys key in Enum.GetValues(typeof(Keys)))
        {
            if (key == Keys.Unknown)
            {
                continue;
            }
            io.AddKeyEvent(TranslateKey(key), KeyboardState.IsKeyDown(key));
        }

        foreach (var c in PressedChars)
        {
            io.AddInputCharacter(c);
        }
        PressedChars.Clear();

        io.KeyCtrl = KeyboardState.IsKeyDown(Keys.LeftControl) || KeyboardState.IsKeyDown(Keys.RightControl);
        io.KeyAlt = KeyboardState.IsKeyDown(Keys.LeftAlt) || KeyboardState.IsKeyDown(Keys.RightAlt);
        io.KeyShift = KeyboardState.IsKeyDown(Keys.LeftShift) || KeyboardState.IsKeyDown(Keys.RightShift);
        io.KeySuper = KeyboardState.IsKeyDown(Keys.LeftSuper) || KeyboardState.IsKeyDown(Keys.RightSuper);
    }

    internal void PressChar(char keyChar)
    {
        PressedChars.Add(keyChar);
    }

    internal void MouseScroll(OpenTK.Mathematics.Vector2 offset)
    {
        ImGuiIOPtr io = ImGui.GetIO();

        io.MouseWheel = offset.Y;
        io.MouseWheelH = offset.X;
    }

    private unsafe void RenderImDrawData(ImDrawDataPtr draw_data)
    {
        if (draw_data.CmdListsCount == 0)
        {
            return;
        }

        _vertexBuffer.Bind();
        for (int i = 0; i < draw_data.CmdListsCount; i++)
        {
            ImDrawListPtr cmd_list = draw_data.CmdLists[i];

            int vertexSize = cmd_list.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>();
            if (vertexSize > _vertexBufferSize)
            {
                int newSize = (int)Math.Max(_vertexBufferSize * 1.5f, vertexSize);

                _vertexBuffer.Dispose();
                _vertexBuffer = _deviceApi.CreateVertexBuffer(newSize, BufferHint.Dynamic);

                _vertexBufferSize = newSize;

                Console.WriteLine($"Resized dear imgui vertex buffer to new size {_vertexBufferSize}");
            }

            int indexSize = cmd_list.IdxBuffer.Size * sizeof(ushort);
            if (indexSize > _indexBufferSize)
            {
                int newSize = (int)Math.Max(_indexBufferSize * 1.5f, indexSize);

                _indexBufferSize = newSize;

                _indexArrayBuffer.Dispose();
                _indexArrayBuffer = _deviceApi.CreateIndexBuffer<ushort>(newSize / sizeof(ushort), BufferHint.Dynamic);

                Console.WriteLine($"Resized dear imgui index buffer to new size {_indexBufferSize}");
            }
        }

        // Setup orthographic projection matrix into our constant buffer
        ImGuiIOPtr io = ImGui.GetIO();
        Matrix4x4 mvp = Matrix4x4.CreateOrthographicOffCenter(
            0.0f,
            io.DisplaySize.X,
            io.DisplaySize.Y,
            0.0f,
            -1.0f,
            1.0f);

        _projectionuniformBlockBuffer.SetData(mvp, 0);
        _shaderProgram.SetUniformBlock(0, _projectionuniformBlockBuffer);

        draw_data.ScaleClipRects(io.DisplayFramebufferScale);

        // Render command lists
        for (int n = 0; n < draw_data.CmdListsCount; n++)
        {
            ImDrawListPtr cmd_list = draw_data.CmdLists[n];

            _vertexBuffer.SetData(0, new ReadOnlySpan<byte>((void*)cmd_list.VtxBuffer.Data, cmd_list.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>()));
            _indexArrayBuffer.SetData(0, new ReadOnlySpan<ushort>((void*)cmd_list.IdxBuffer.Data, cmd_list.IdxBuffer.Size));

            for (int cmd_i = 0; cmd_i < cmd_list.CmdBuffer.Size; cmd_i++)
            {
                ImDrawCmdPtr pcmd = cmd_list.CmdBuffer[cmd_i];
                if (pcmd.UserCallback != IntPtr.Zero)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    var tex = _deviceApi.GetTextureById((int)pcmd.TextureId);
                    tex.SetSlot(0);
                    tex.Bind();

                    // We do _windowHeight - (int)clip.W instead of (int)clip.Y because gl has flipped Y when it comes to these coordinates
                    var clip = pcmd.ClipRect;
                    _drawContext.ScissorRect = new Rectangle((int)clip.X, _windowHeight - (int)clip.W, (int)(clip.Z - clip.X), (int)(clip.W - clip.Y));

                    if ((io.BackendFlags & ImGuiBackendFlags.RendererHasVtxOffset) != 0)
                    {
                        _drawContext.BaseVertex = (int)pcmd.VtxOffset;
                        _drawContext.Offset = (int)pcmd.IdxOffset;
                        _drawContext.Lenght = (int)pcmd.ElemCount;
                        _drawContext.Layout = _layout;
                        _drawContext.ShaderProgram = _shaderProgram;
                        _drawContext.VertexBuffer = _vertexBuffer;
                        _drawContext.IndexBuffer = _indexArrayBuffer;

                        _deviceApi.DrawTriangles(_drawContext);
                    }
                    else
                    {
                        _drawContext.BaseVertex = null;
                        _drawContext.Offset = (int)pcmd.IdxOffset;
                        _drawContext.Lenght = (int)pcmd.ElemCount;
                        _drawContext.Layout = _layout;
                        _drawContext.ShaderProgram = _shaderProgram;
                        _drawContext.VertexBuffer = _vertexBuffer;
                        _drawContext.IndexBuffer = _indexArrayBuffer;

                        _deviceApi.DrawTriangles(_drawContext);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Frees all graphics resources used by the renderer.
    /// </summary>
    public void Dispose()
    {
        _vertexBuffer.Dispose();
        _indexArrayBuffer.Dispose();

        _fontTexture.Dispose();
        _shaderProgram.Dispose();
    }
    public static ImGuiKey TranslateKey(Keys key)
    {
        if (key >= Keys.D0 && key <= Keys.D9)
            return key - Keys.D0 + ImGuiKey._0;

        if (key >= Keys.A && key <= Keys.Z)
            return key - Keys.A + ImGuiKey.A;

        if (key >= Keys.KeyPad0 && key <= Keys.KeyPad9)
            return key - Keys.KeyPad0 + ImGuiKey.Keypad0;

        if (key >= Keys.F1 && key <= Keys.F24)
            return key - Keys.F1 + ImGuiKey.F24;

        switch (key)
        {
            case Keys.Tab: return ImGuiKey.Tab;
            case Keys.Left: return ImGuiKey.LeftArrow;
            case Keys.Right: return ImGuiKey.RightArrow;
            case Keys.Up: return ImGuiKey.UpArrow;
            case Keys.Down: return ImGuiKey.DownArrow;
            case Keys.PageUp: return ImGuiKey.PageUp;
            case Keys.PageDown: return ImGuiKey.PageDown;
            case Keys.Home: return ImGuiKey.Home;
            case Keys.End: return ImGuiKey.End;
            case Keys.Insert: return ImGuiKey.Insert;
            case Keys.Delete: return ImGuiKey.Delete;
            case Keys.Backspace: return ImGuiKey.Backspace;
            case Keys.Space: return ImGuiKey.Space;
            case Keys.Enter: return ImGuiKey.Enter;
            case Keys.Escape: return ImGuiKey.Escape;
            case Keys.Apostrophe: return ImGuiKey.Apostrophe;
            case Keys.Comma: return ImGuiKey.Comma;
            case Keys.Minus: return ImGuiKey.Minus;
            case Keys.Period: return ImGuiKey.Period;
            case Keys.Slash: return ImGuiKey.Slash;
            case Keys.Semicolon: return ImGuiKey.Semicolon;
            case Keys.Equal: return ImGuiKey.Equal;
            case Keys.LeftBracket: return ImGuiKey.LeftBracket;
            case Keys.Backslash: return ImGuiKey.Backslash;
            case Keys.RightBracket: return ImGuiKey.RightBracket;
            case Keys.GraveAccent: return ImGuiKey.GraveAccent;
            case Keys.CapsLock: return ImGuiKey.CapsLock;
            case Keys.ScrollLock: return ImGuiKey.ScrollLock;
            case Keys.NumLock: return ImGuiKey.NumLock;
            case Keys.PrintScreen: return ImGuiKey.PrintScreen;
            case Keys.Pause: return ImGuiKey.Pause;
            case Keys.KeyPadDecimal: return ImGuiKey.KeypadDecimal;
            case Keys.KeyPadDivide: return ImGuiKey.KeypadDivide;
            case Keys.KeyPadMultiply: return ImGuiKey.KeypadMultiply;
            case Keys.KeyPadSubtract: return ImGuiKey.KeypadSubtract;
            case Keys.KeyPadAdd: return ImGuiKey.KeypadAdd;
            case Keys.KeyPadEnter: return ImGuiKey.KeypadEnter;
            case Keys.KeyPadEqual: return ImGuiKey.KeypadEqual;
            case Keys.LeftShift: return ImGuiKey.LeftShift;
            case Keys.LeftControl: return ImGuiKey.LeftCtrl;
            case Keys.LeftAlt: return ImGuiKey.LeftAlt;
            case Keys.LeftSuper: return ImGuiKey.LeftSuper;
            case Keys.RightShift: return ImGuiKey.RightShift;
            case Keys.RightControl: return ImGuiKey.RightCtrl;
            case Keys.RightAlt: return ImGuiKey.RightAlt;
            case Keys.RightSuper: return ImGuiKey.RightSuper;
            case Keys.Menu: return ImGuiKey.Menu;
            default: return ImGuiKey.None;
        }
    }
}