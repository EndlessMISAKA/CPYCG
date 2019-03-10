using System.Collections.Generic;
using System.IO;

namespace WpfAnimatedGif
{
    public abstract class GifExtension : GifBlock
    {
        public static GifExtension ReadExtension(Stream stream, IEnumerable<GifExtension> controlExtensions, bool metadataOnly)
        {
            // Note: at this point, the Extension Introducer (0x21) has already been read
            int label = stream.ReadByte();
            if (label < 0)
                throw GifHelpers.UnexpectedEndOfStreamException();
            switch (label)
            {
                case GifHelpers.Extension_GifGraphicControl:
                    return new GifGraphicControlExtension(stream);
                case GifHelpers.Extension_GifComment:
                    return new GifCommentExtension(stream);
                case GifHelpers.Extension_GifPlainText:
                    return new GifPlainTextExtension(stream, controlExtensions, metadataOnly);
                case GifHelpers.Extension_GifApplication:
                    return new GifApplicationExtension(stream);
                default:
                    throw GifHelpers.UnknownExtensionTypeException(label);
            }
        }
    }
}
