namespace PixelGenesis._3D.Renderer.DeviceApi.Abstractions;

public interface ITexture : IDeviceObject
{
    void SetSlot(int slot);
}

public enum PGPixelFormat
{
    Rgba,
    Bgra
}

public enum PGInternalPixelFormat
{
    Rgba,
    Rgba8
}

public enum PGPixelType
{
    UnsignedByte
}

