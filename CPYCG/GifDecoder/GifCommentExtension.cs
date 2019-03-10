using System.IO;
using System.Text;

namespace WpfAnimatedGif
{
    public class GifCommentExtension : GifExtension
    {
        public string Text { get; private set; }

        public GifCommentExtension() { }

        public GifCommentExtension(Stream stream)
        {
            Read(stream);
        }

        public override GifBlockKind Kind
        {
            get { return GifBlockKind.SpecialPurpose; }
        }

        //public GifCommentExtension ReadComment(Stream stream)
        //{
        //    var comment = new GifCommentExtension();
        //    comment.Read(stream);
        //    return comment;
        //}

        public void Read(Stream stream)
        {
            // Note: at this point, the label (0xFE) has already been read
            var bytes = GifHelpers.ReadDataBlocks(stream, false);
            if (bytes != null)
                Text = Encoding.ASCII.GetString(bytes);
        }
    }
}
