using System;
using System.IO;
using System.Text;

namespace WpfAnimatedGif.Decoding
{
    // label 0xFF
    internal class GifApplicationExtension : GifExtension
    {
        internal const int ExtensionLabel = 0xFF;

        public int BlockSize { get; private set; }
        public string ApplicationIdentifier { get; private set; }
        public byte[] AuthenticationCode { get; private set; }
        public byte[] Data { get; private set; }

        private GifApplicationExtension()
        {
        }

        internal override GifBlockKind Kind
        {
            get { return GifBlockKind.SpecialPurpose; }
        }

        internal static GifApplicationExtension ReadApplication(Stream stream)
        {
            var ext = new GifApplicationExtension();
            ext.Read(stream);
            return ext;
        }

        private void Read(Stream stream)
        {
            // Note: at this point, the label (0xFF) has already been read

            byte[] bytes = new byte[12];
            stream.ReadAll(bytes, 0, bytes.Length);
            BlockSize = bytes[0]; // should always be 11
            if (BlockSize != 11)
                throw GifHelpers.InvalidBlockSizeException("Application Extension", 11, BlockSize);

            ApplicationIdentifier = Encoding.ASCII.GetString(bytes, 1, 8);
            byte[] authCode = new byte[3];
            Array.Copy(bytes, 9, authCode, 0, 3);
            AuthenticationCode = authCode;
            Data = GifHelpers.ReadDataBlocks(stream, false);
        }
    }
}
