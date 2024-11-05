using OpenTK.Graphics.OpenGL4;
using StbImageSharp;


namespace PixelGenesis.Lab.OpenGLAbstractions;

internal class Texture : IDisposable
{
    int _rendererId;

    public int Width { get; private set; }
    public int Height { get; private set; }

    public Texture(string path)
    {
        StbImage.stbi_set_flip_vertically_on_load(1);

        GL.GenTextures(1, out _rendererId);
        GL.BindTexture(TextureTarget.Texture2D, _rendererId);

        var image = ImageResult.FromStream(File.OpenRead(path), ColorComponents.RedGreenBlueAlpha);

        Width = image.Width;
        Height = image.Height;

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
    }

    public void Bind(int slot)
    {
        GL.ActiveTexture(TextureUnit.Texture0 + slot);
        GL.BindTexture(TextureTarget.Texture2D, _rendererId);
    }

    public void Unbind()
    {
        GL.BindTexture(TextureTarget.Texture2D, 0);
    }

    public void Dispose()
    {
        GL.DeleteTexture(_rendererId);
    }
}
