using System;
using System.IO;

namespace WpfAnimatedGif
{
    // label 0xF9
    public class GifGraphicControlExtension : GifExtension
    {
        public int BlockSize { get; private set; }
        public int DisposalMethod { get; private set; }
        public bool UserInput { get; private set; }
        public bool HasTransparency { get; private set; }
        public int Delay { get; private set; }
        public int TransparencyIndex { get; private set; }

        public GifGraphicControlExtension() { }

        public GifGraphicControlExtension(Stream stream)
        {
            Read(stream);
        }

        public override GifBlockKind Kind
        {
            get { return GifBlockKind.Control; }
        }

        //public GifGraphicControlExtension ReadGraphicsControl(Stream stream)
        //{
        //    var ext = new GifGraphicControlExtension();
        //    ext.Read(stream);
        //    return ext;
        //}

        public void Read(Stream stream)
        {
            // Note: at this point, the label (0xF9) has already been read
            byte[] bytes = new byte[6];
            stream.Read(bytes, 0, bytes.Length);
            BlockSize = bytes[0]; // should always be 4
            if (BlockSize != 4)
                return;
                //throw GifHelpers.InvalidBlockSizeException("Graphic Control Extension", 4, BlockSize);
            byte packedFields = bytes[1];
            DisposalMethod = (packedFields & 0x1C) >> 2;
            UserInput = (packedFields & 0x02) != 0;
            HasTransparency = (packedFields & 0x01) != 0;
            Delay = BitConverter.ToUInt16(bytes, 2) * 10; // milliseconds
            TransparencyIndex = bytes[4];
        }
    }
}
