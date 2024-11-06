using PixelGenesis.ECS;

namespace PixelGenesis._3D.Common;

[ReadableAsset<Texture, Factory>]
public class Texture : IWritableAsset, IReadableAsset
{
    public string Reference { get; }
    public int Widht { get; }
    public int Height { get; }
    public ReadOnlyMemory<byte> Data { get; }

    public Texture(int width, int height, ReadOnlyMemory<byte> data, string reference)
    {
        Widht = width;
        Height = height;
        Data = data;
        Reference = reference;
    }   

    public void WriteToStream(Stream stream)
    {
        var bw = new BinaryWriter(stream);
        bw.Write(Widht);
        bw.Write(Height);
        bw.Write(Data.Length);
        bw.Write(Data.Span);
    }

    public class Factory : IReadableAssetFactory<Texture>
    {
        public Texture ReadAsset(string reference, Stream stream)
        {
            var br = new BinaryReader(stream);
            var width = br.ReadInt32();
            var height = br.ReadInt32();
            var length = br.ReadInt32();
            var data = br.ReadBytes(length);

            return new Texture(width, height, data, reference);
        }
    }
}
