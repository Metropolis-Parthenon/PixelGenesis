using ImGuiNET;
using OpenTK.Graphics.ES11;
using System.Numerics;

namespace PixelGenesis.Lab.Tests;

internal class ClearColorTest : ITest
{
    Vector4 Color = new Vector4(0.2f, 0.3f, 0.8f, 1.0f);

    public ClearColorTest()
    {

    }

    public void OnUpdate(float deltaTime)
    {
    }

    public void OnGui()
    {
        ImGui.ColorEdit4("Clear Color", ref Color);
    }

    public void OnRender()
    {
        GL.ClearColor(Color.X, Color.Y, Color.Z, Color.W);
        GL.Clear(ClearBufferMask.ColorBufferBit);
    }

    public void Dispose()
    {
    }

}
