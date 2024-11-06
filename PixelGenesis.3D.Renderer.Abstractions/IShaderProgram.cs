namespace PixelGenesis._3D.Renderer.DeviceApi.Abstractions;

public interface IShaderProgram : IDeviceObject
{
    public void SetUniformBlock(int binding, IUniformBlockBuffer buffer);
}