using System;

namespace WpfAnimatedGif.Decoding
{
    [Serializable]
    internal class GifDecoderException : Exception
    {
        internal GifDecoderException() { }
        internal GifDecoderException(string message) : base(message) { }
        internal GifDecoderException(string message, Exception inner) : base(message, inner) { }
        protected GifDecoderException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
