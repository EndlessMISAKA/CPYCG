using System.IO;

namespace WpfAnimatedGif
{
    public class GifHeader : GifBlock
    {
        public string Signature { get; private set; }
        public string Version { get; private set; }
        public GifLogicalScreenDescriptor LogicalScreenDescriptor { get; private set; }

        public GifHeader() { }

        public GifHeader(Stream stream)
        {
            Read(stream);
        }

        public override GifBlockKind Kind
        {
            get { return GifBlockKind.Other; }
        }

        //public GifHeader ReadHeader(Stream stream)
        //{
        //    var header = new GifHeader();
        //    header.Read(stream);
        //    return header;
        //}

        public void Read(Stream stream)
        {
            Signature = GifHelpers.ReadString(stream, 3);
            if (Signature != "GIF")
                return;
                //throw GifHelpers.InvalidSignatureException(Signature);
            Version = GifHelpers.ReadString(stream, 3);
            if (Version != "87a" && Version != "89a")
                return;
                //throw GifHelpers.UnsupportedVersionException(Version);
            LogicalScreenDescriptor = new GifLogicalScreenDescriptor(stream);
        }
    }
}
