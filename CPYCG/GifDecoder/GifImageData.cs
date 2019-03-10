using System.IO;

namespace WpfAnimatedGif
{
    public class GifImageData
    {
        public byte LzwMinimumCodeSize { get; set; }
        public byte[] CompressedData { get; set; }

        public GifImageData() { }

        //public GifImageData ReadImageData(Stream stream, bool metadataOnly)
        //{
        //    var imgData = new GifImageData();
        //    imgData.Read(stream, metadataOnly);
        //    return imgData;
        //}

        public void Read(Stream stream, bool metadataOnly)
        {
            LzwMinimumCodeSize = (byte)stream.ReadByte();
            CompressedData = GifHelpers.ReadDataBlocks(stream, metadataOnly);
        }
    }
}
