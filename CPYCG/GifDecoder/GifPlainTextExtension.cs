using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WpfAnimatedGif
{
    // label 0x01
    public class GifPlainTextExtension : GifExtension
    {
        public int BlockSize { get; private set; }
        public int Left { get; private set; }
        public int Top { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int CellWidth { get; private set; }
        public int CellHeight { get; private set; }
        public int ForegroundColorIndex { get; private set; }
        public int BackgroundColorIndex { get; private set; }
        public string Text { get; private set; }

        public IList<GifExtension> Extensions { get; private set; }

        public GifPlainTextExtension() { }

        public GifPlainTextExtension(Stream stream, IEnumerable<GifExtension> controlExtensions, bool metadataOnly)
        {
            Read(stream, controlExtensions, metadataOnly);
        }

        public override GifBlockKind Kind
        {
            get { return GifBlockKind.GraphicRendering; }
        }

        //public GifPlainTextExtension ReadPlainText(Stream stream, IEnumerable<GifExtension> controlExtensions, bool metadataOnly)
        //{
        //    var plainText = new GifPlainTextExtension();
        //    plainText.Read(stream, controlExtensions, metadataOnly);
        //    return plainText;
        //}

        public void Read(Stream stream, IEnumerable<GifExtension> controlExtensions, bool metadataOnly)
        {
            // Note: at this point, the label (0x01) has already been read
            byte[] bytes = new byte[13];
            stream.Read(bytes,0, bytes.Length);

            BlockSize = bytes[0];
            if (BlockSize != 12)
                throw GifHelpers.InvalidBlockSizeException("Plain Text Extension", 12, BlockSize);

            Left = BitConverter.ToUInt16(bytes, 1);
            Top = BitConverter.ToUInt16(bytes, 3);
            Width = BitConverter.ToUInt16(bytes, 5);
            Height = BitConverter.ToUInt16(bytes, 7);
            CellWidth = bytes[9];
            CellHeight = bytes[10];
            ForegroundColorIndex = bytes[11];
            BackgroundColorIndex = bytes[12];

            var dataBytes = GifHelpers.ReadDataBlocks(stream, metadataOnly);
            Text = Encoding.ASCII.GetString(dataBytes);
            if(null != controlExtensions)
                Extensions = controlExtensions.ToList().AsReadOnly();
        }
    }
}
