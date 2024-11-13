using PixelGenesis._3D.Common;
using PixelGenesis._3D.Renderer.DeviceApi.Abstractions;

namespace PixelGenesis._3D.Renderer;

public class DeviceShader : IDisposable
{
    public CompiledShader CompiledShader { get; private set; }
    public IShaderProgram ShaderProgram { get; private set; }

    public DeviceShader(IDeviceApi deviceApi, CompiledShader compiledShader)
    {
        ShaderProgram = deviceApi.CreateShaderProgram(
            compiledShader.Vertex,
            compiledShader.Fragment,
            compiledShader.Tessellation,
            compiledShader.Geometry            
        );

        CompiledShader = compiledShader;
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
