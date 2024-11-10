using System.Drawing;
using System.Reflection;

namespace EarTrumpet.Extensions
{
    public static class IconExtensions
    {
        public static Icon AsDisposableIcon(this Icon icon)
        {
            // System.Drawing.Icon does not expose a method to declare
            // ownership of its wrapped handle so we have to use reflection
            // here.

            // See also: https://github.com/dotnet/runtime/blob/main/src/libraries/System.Drawing.Common/src/System/Drawing/Icon.Windows.cs#L42
            icon.GetType().GetField("_ownHandle", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(icon, true);
            return icon;
        }
    }
}
