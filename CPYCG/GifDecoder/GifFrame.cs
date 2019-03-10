using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WpfAnimatedGif
{
    public class GifFrame : GifBlock
    {
        public GifImageDescriptor Descriptor { get; private set; }
        public GifColor[] LocalColorTable { get; private set; }
        public IList<GifExtension> Extensions { get; private set; }
        public GifImageData ImageData { get; private set; }

        public GifFrame() { }

        public GifFrame(Stream stream, IEnumerable<GifExtension> controlExtensions, bool metadataOnly)
        {
            Read(stream, controlExtensions, metadataOnly);
        }

        public override GifBlockKind Kind
        {
            get { return GifBlockKind.GraphicRendering; }
        }

        //public GifFrame ReadFrame(Stream stream, IEnumerable<GifExtension> controlExtensions, bool metadataOnly)
        //{
        //    var frame = new GifFrame();

        //    frame.Read(stream, controlExtensions, metadataOnly);

        //    return frame;
        //}

        public void Read(Stream stream, IEnumerable<GifExtension> controlExtensions, bool metadataOnly)
        {
            // Note: at this point, the Image Separator (0x2C) has already been read
            Descriptor = new GifImageDescriptor();
            Descriptor.Read(stream);
            if (Descriptor.HasLocalColorTable)
            {
                LocalColorTable = GifHelpers.ReadColorTable(stream, Descriptor.LocalColorTableSize);
            }
            ImageData = new GifImageData();
            ImageData.Read(stream, metadataOnly);
            Extensions = controlExtensions.ToList().AsReadOnly();
        }
    }
}
