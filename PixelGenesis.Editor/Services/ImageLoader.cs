using OpenTK.Graphics.OpenGL4;
using PixelGenesis._3D.Renderer.DeviceApi.Abstractions;
using StbImageSharp;

namespace PixelGenesis.Editor.Services;

internal class ImageLoader(IDeviceApi deviceApi)
{
    Dictionary<string, int> ImageTextures = new Dictionary<string, int>();

    public int LoadImage(string path)
    {
        if(ImageTextures.TryGetValue(path, out var textureId))
        {            
            return textureId;
        }

        var image = ImageResult.FromStream(File.OpenRead(path), ColorComponents.RedGreenBlueAlpha);

        var texture = deviceApi.CreateTexture(image.Width, image.Height, image.Data, PGPixelFormat.Rgba, PGInternalPixelFormat.Rgba, PGPixelType.UnsignedByte);

        textureId = texture.Id;

        //textureId = GL.GenTexture();
        //GL.BindTexture(TextureTarget.Texture2D, textureId);

        //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

        //GL.PixelStore(PixelStoreParameter.UnpackRowLength, 0);


        //GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);

        ImageTextures.Add(path, textureId);

        return textureId;
    }

}
