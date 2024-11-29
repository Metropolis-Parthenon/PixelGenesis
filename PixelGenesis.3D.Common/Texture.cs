using PixelGenesis._3D.Renderer.DeviceApi.Abstractions;
using PixelGenesis.ECS.AssetManagement;
using StbImageSharp;

namespace PixelGenesis._3D.Common;

public class Texture : IAsset
{
    public Guid Id { get; }
    public string Name { get; }
    public int Width { get; }
    public int Height { get; }
    public PGPixelFormat PixelFormat { get; }
    public ReadOnlyMemory<byte> Data { get; }

    public Texture(Guid id, int width, int height, ReadOnlyMemory<byte> data, PGPixelFormat pGPixelFormat, string? name = default)
    {
        Id = id;
        Name = name ?? $"{id}.pgtex";
        Width = width;
        Height = height;
        Data = data;
    }   

    public void WriteToStream(IAssetManager assetManager, Stream stream)
    {
        var bw = new BinaryWriter(stream);
        bw.Write((int)PixelFormat);
        bw.Write(Width);
        bw.Write(Height);
        bw.Write(Data.Length);
        bw.Write(Data.Span);
    }

    public static Texture FromImageFile(string file, ColorComponents colorComponents, string? name = default)
    {
        //StbImage.stbi_set_flip_vertically_on_load(1);

        using var fileStream = File.OpenRead(file);

        var image = ImageResult.FromStream(fileStream, colorComponents);

        return new Texture(Guid.NewGuid(), image.Width, image.Height, image.Data, image.Comp switch
        {
            ColorComponents.RedGreenBlue => PGPixelFormat.Rgb,
            ColorComponents.RedGreenBlueAlpha => PGPixelFormat.Rgba,
            _ => throw new NotImplementedException()
        }, name);
    }

    public class Factory : IReadAssetFactory
    {
        public IAsset ReadAsset(Guid id, IAssetManager assetManager, Stream stream)
        {
            var br = new BinaryReader(stream);
            var pixelFormat = (PGPixelFormat)br.ReadInt32();
            var width = br.ReadInt32();
            var height = br.ReadInt32();
            var length = br.ReadInt32();
            var data = br.ReadBytes(length);

            return new Texture(id, width, height, data, pixelFormat);
        }
    }
}
