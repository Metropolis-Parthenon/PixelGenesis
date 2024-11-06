using OpenTK.Graphics.OpenGL4;
using PixelGenesis._3D.Renderer.DeviceApi.Abstractions;
using System.Runtime.CompilerServices;

namespace PixelGenesis._3D.Renderer.DeviceApi.OpenGL;

internal class GLTexture : ITexture
{
    int _id;
    public int Id => _id;

    OpenGLDeviceApi _api;

    public unsafe GLTexture(
        int width, 
        int height, 
        ReadOnlyMemory<byte> data, 
        PixelFormat pixelFormat, 
        PixelInternalFormat pixelInternalFormat, 
        PixelType pixelType, 
        OpenGLDeviceApi api)
    {
        _api = api;

        GL.GenTextures(1, out _id);
        OpenGLDeviceApi.ThrowOnGLError();
        GL.BindTexture(TextureTarget.Texture2D, _id);
        OpenGLDeviceApi.ThrowOnGLError();

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        OpenGLDeviceApi.ThrowOnGLError();
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        OpenGLDeviceApi.ThrowOnGLError();

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        OpenGLDeviceApi.ThrowOnGLError();
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        OpenGLDeviceApi.ThrowOnGLError();

        IntPtr dataPtr;
        fixed(byte* ptr = data.Span)
        {
            dataPtr = (IntPtr)ptr;
        }

        GL.TexImage2D(TextureTarget.Texture2D, 0, pixelInternalFormat, width, height, 0, pixelFormat, pixelType, dataPtr);
        OpenGLDeviceApi.ThrowOnGLError();
        _api._textures.Add(_id, this);
    }

    public void SetSlot(int slot)
    {
        GL.ActiveTexture(TextureUnit.Texture0 + slot);
        OpenGLDeviceApi.ThrowOnGLError();
    }

    public void Bind()
    {
        GL.BindTexture(TextureTarget.Texture2D, _id);
        OpenGLDeviceApi.ThrowOnGLError();
    }
    public void Unbind()
    {
        GL.BindTexture(TextureTarget.Texture2D, 0);
        OpenGLDeviceApi.ThrowOnGLError();
    }

    public void Dispose()
    {
        GL.DeleteTexture(_id);
        OpenGLDeviceApi.ThrowOnGLError();
        _api._textures.Remove(_id);
    }

}
