namespace PixelGenesis._3D.Renderer.DeviceApi.Abstractions;

public interface ITexture : IDeviceObject
{
    void SetSlot(int slot);
}

public enum PGPixelFormat
{
    Rgba,
    Bgra,
    Rgb
}

public enum PGInternalPixelFormat
{
    Rgba,
    Rgba8,
    Rgb
}

public enum PGPixelType
{
    UnsignedByte
}

public interface ICubemapTexture : IDeviceObject
{
    void SetSlot(int slot);
}