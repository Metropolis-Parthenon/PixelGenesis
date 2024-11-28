using PixelGenesis._3D.Common;
using PixelGenesis._3D.Renderer.DeviceApi.Abstractions;

namespace PixelGenesis._3D.Renderer.DeviceObjects;

internal class RendererDeviceTexture(IDeviceApi deviceApi, Texture texture) : IRendererDeviceObject
{
    public Texture Texture => texture;
    public ITexture DeviceTexture { get; private set; }
    public void Initialize()
    {
        DeviceTexture = deviceApi.CreateTexture(
                    texture.Width,
                    texture.Height,
                    texture.Data,
                    texture.PixelFormat,
                    PGInternalPixelFormat.Rgba,
                    PGPixelType.UnsignedByte);
    }

    public void Update() { }
    
    public void AfterUpdate() { }
    
    public void Dispose()
    {
        DeviceTexture?.Dispose();
    }

}
