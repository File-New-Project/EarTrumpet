using System.Drawing;
using Windows.Win32.Foundation;

namespace EarTrumpet.Extensions;
public static class RectExtensions
{
    public static bool Contains(this RECT rect, Point pt)
    {
        return pt.X >= rect.left && pt.X <= rect.right && pt.Y >= rect.top && pt.Y <= rect.bottom;
    }
}