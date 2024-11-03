using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

namespace PixelGenesis.Editor.Services;

internal class ImageLoader
{
    Dictionary<string, int> ImageTextures = new Dictionary<string, int>();

    public int LoadImage(string path)
    {
        if(ImageTextures.TryGetValue(path, out var textureId))
        {            
            return textureId;
        }
                
        textureId = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, textureId);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

        GL.PixelStore(PixelStoreParameter.UnpackRowLength, 0);

        var image = ImageResult.FromStream(File.OpenRead(path), ColorComponents.RedGreenBlueAlpha);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);

        ImageTextures.Add(path, textureId);

        return textureId;
    }

}
