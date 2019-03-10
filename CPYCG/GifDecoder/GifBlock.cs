using System.Collections.Generic;
using System.IO;

namespace WpfAnimatedGif
{
    public abstract class GifBlock
    {
        public static GifBlock ReadBlock(Stream stream, IEnumerable<GifExtension> controlExtensions, bool metadataOnly)
        {
            int blockId = stream.ReadByte();
            if (blockId < 0)
                return new GifTrailer();
                //throw GifHelpers.UnexpectedEndOfStreamException();   //20180126 gif文件损坏
            switch (blockId)
            {
                case GifHelpers.ExtensionIntroducer:
                    return GifExtension.ReadExtension(stream, controlExtensions, metadataOnly);
                case GifHelpers.ImageSeparator:
                    return new GifFrame(stream, controlExtensions, metadataOnly);
                case GifHelpers.TrailerByte:
                    return new GifTrailer();
                default:
                    return new GifTrailer();
                    //throw GifHelpers.UnknownBlockTypeException(blockId);  //20180126 gif文件损坏
            }
        }

        public abstract GifBlockKind Kind { get; }
    }
}
