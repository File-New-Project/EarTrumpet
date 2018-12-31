using System.IO;

namespace WpfAnimatedGif.Decoding
{
    internal class GifHeader : GifBlock
    {
        public string Signature { get; private set; }
        public string Version { get; private set; }
        public GifLogicalScreenDescriptor LogicalScreenDescriptor { get; private set; }

        private GifHeader()
        {
        }

        internal override GifBlockKind Kind
        {
            get { return GifBlockKind.Other; }
        }

        internal static GifHeader ReadHeader(Stream stream)
        {
            var header = new GifHeader();
            header.Read(stream);
            return header;
        }

        private void Read(Stream stream)
        {
            Signature = GifHelpers.ReadString(stream, 3);
            if (Signature != "GIF")
                throw GifHelpers.InvalidSignatureException(Signature);
            Version = GifHelpers.ReadString(stream, 3);
            if (Version != "87a" && Version != "89a")
                throw GifHelpers.UnsupportedVersionException(Version);
            LogicalScreenDescriptor = GifLogicalScreenDescriptor.ReadLogicalScreenDescriptor(stream);
        }
    }
}
