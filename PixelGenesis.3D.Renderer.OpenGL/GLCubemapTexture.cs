using OpenTK.Graphics.OpenGL4;
using PixelGenesis._3D.Renderer.DeviceApi.Abstractions;

namespace PixelGenesis._3D.Renderer.DeviceApi.OpenGL;

public class GLCubemapTexture : ICubemapTexture
{
    const int TextureFacesCount = 6;

    OpenGLDeviceApi _api;

    int _id;
    public int Id => _id;

    public unsafe GLCubemapTexture(
        ReadOnlySpan<(int Width, int Height)> dimensions,
        ReadOnlySpan<ReadOnlyMemory<byte>> datas,
        PixelInternalFormat internalFormat,
        PixelFormat pixelFormat,
        PixelType pixelType,
        OpenGLDeviceApi api
        )
    {

        if (dimensions.Length is not TextureFacesCount || datas.Length is not TextureFacesCount)
        {
            throw new InvalidOperationException("Cube map texture must have 6 textures");
        }

        _id = GL.GenTexture();
        OpenGLDeviceApi.ThrowOnGLError();

        GL.BindTexture(TextureTarget.TextureCubeMap, _id);
        OpenGLDeviceApi.ThrowOnGLError();


        for (var i = 0; i < TextureFacesCount; i++)
        {
            var data = datas[i];
            var (width, height) = dimensions[i];

            //IntPtr dataPtr;
            //fixed (byte* ptr = data.Span)
            //{
            //    dataPtr = (IntPtr)ptr;
            //}

            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, PixelInternalFormat.Rgb, width, height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, data.ToArray());
            OpenGLDeviceApi.ThrowOnGLError();
        }

        GL.TexParameterI(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, [(int)TextureMinFilter.Linear]);
        OpenGLDeviceApi.ThrowOnGLError();
        GL.TexParameterI(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, [(int)TextureMinFilter.Linear]);
        OpenGLDeviceApi.ThrowOnGLError();
        GL.TexParameterI(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, [(int)TextureWrapMode.ClampToEdge]);
        OpenGLDeviceApi.ThrowOnGLError();
        GL.TexParameterI(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, [(int)TextureWrapMode.ClampToEdge]);
        OpenGLDeviceApi.ThrowOnGLError();
        GL.TexParameterI(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, [(int)TextureWrapMode.ClampToEdge]);

        _api = api;
        api._cubemaps.Add(_id, this);
    }

    public void SetSlot(int slot)
    {
        GL.ActiveTexture(TextureUnit.Texture0 + slot);
        OpenGLDeviceApi.ThrowOnGLError();
    }

    public void Bind()
    {
        GL.BindTexture(TextureTarget.TextureCubeMap, _id);
        OpenGLDeviceApi.ThrowOnGLError();
    }

    public void Unbind()
    {
        GL.BindTexture(TextureTarget.TextureCubeMap, 0);
        OpenGLDeviceApi.ThrowOnGLError();
    }


    public void Dispose()
    {
        GL.DeleteTexture(_id);
        OpenGLDeviceApi.ThrowOnGLError();
        _api._cubemaps.Remove(_id);
    }

}