namespace WpfAnimatedGif
{
    public class GifTrailer : GifBlock
    {
        public GifTrailer() { }

        public override GifBlockKind Kind
        {
            get { return GifBlockKind.Other; }
        }
    }
}
