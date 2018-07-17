using Microsoft.Win32;

namespace EarTrumpet.Extensions
{
    public static class RegistryKeyExtensions
    {
        public static T GetValue<T>(this RegistryKey self, string valueName, T defaultValue)
        {
            // Manually apply defaultValue if the data type is mismatched.
            var value = self.GetValue(valueName, defaultValue);
            if (value is T)
            {
                return (T)value;
            }
            else
            {
                return defaultValue;
            }
        }
    }
}
