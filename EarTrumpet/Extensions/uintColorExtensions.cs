using System.Windows.Media;

namespace EarTrumpet.Extensions
{
    public static class uintColorExtensions
    {
        public static Color ToABGRColor(this uint abgrValue)
        {
            var colorBytes = new byte[4];
            colorBytes[0] = (byte)((0xFF000000 & abgrValue) >> 24);     // A
            colorBytes[1] = (byte)((0x00FF0000 & abgrValue) >> 16);     // B
            colorBytes[2] = (byte)((0x0000FF00 & abgrValue) >> 8);      // G
            colorBytes[3] = (byte)(0x000000FF & abgrValue);             // R

            return Color.FromArgb(colorBytes[0], colorBytes[3], colorBytes[2], colorBytes[1]);
        }
    }
}
