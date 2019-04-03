using System.Collections.Generic;

namespace EarTrumpet.UI.Themes
{
    public class Rule
    {
        public enum Kind
        {
            Any = 0,
            LightTheme,
            Transparency,
            UseAccentColor,
            UseAccentColorOnWindowBorders,
            HighContrast,
            AccentPolicySupportsTintColor,
        }

        public Kind On { get; set; }
        public string Value { get; set; }
        public List<Rule> Rules { get; } = new List<Rule>();
    }
}
