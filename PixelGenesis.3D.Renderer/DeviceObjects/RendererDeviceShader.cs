using PixelGenesis._3D.Common;
using PixelGenesis._3D.Renderer.DeviceApi.Abstractions;

namespace PixelGenesis._3D.Renderer.DeviceObjects;

public class RendererDeviceShader(IDeviceApi deviceApi, CompiledShader compiledShader) : IRendererDeviceObject
{
    public CompiledShader CompiledShader => compiledShader;
    public IShaderProgram ShaderProgram { get; private set; }

    public void Initialize()
    {
        ShaderProgram = deviceApi.CreateShaderProgram(
            compiledShader.Vertex,
            compiledShader.Fragment,
            compiledShader.Tessellation,
            compiledShader.Geometry
        );
    }

    public void Update() { }
    
    public void AfterUpdate() { }

    public void Dispose()
    {
        ShaderProgram?.Dispose();
    }

}
