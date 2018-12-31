using System.Windows.Media;

namespace EarTrumpet.UI.Themes
{
    public interface IResolveColorOptions
    {
        bool IsHighContrast { get; }
        bool IsLightTheme { get; }
        bool IsTransparencyEnabled { get; }
        bool UseAccentColor { get; }
        Color LookupThemeColor(string color);
    }
}