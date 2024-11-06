namespace PixelGenesis._3D.Renderer.DeviceApi.Abstractions;

public interface IDeviceObject : IDisposable
{
    public int Id { get; }

    public void Bind();

    public void Unbind();
}
