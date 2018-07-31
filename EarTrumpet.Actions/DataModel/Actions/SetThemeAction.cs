using EarTrumpet.Extensibility.Shared;
using System.Collections.Generic;
using System.Linq;

namespace EarTrumpet_Actions.DataModel.Actions
{
    public class SetThemeAction : BaseAction
    {
        public string Theme { get; set; }

        public override string Describe() => $"Set Theme to {Options[0].DisplayName}";

        public SetThemeAction()
        {
            string[] themes = new string[] { };
            var themeService = (dynamic)ServiceBus.Get("EarTrumpet-Themes");
            if (themeService != null)
            {
                themes = themeService.Themes;
                Theme = themeService.Theme;
            }

            Description = Properties.Resources.SetThemeActionDescriptionText;
            Options = new List<OptionData>(new OptionData[]{ new OptionData(
                new List<Option>(themes.Select(t => new Option(t, t))),
                (newValue) => Theme = (string)newValue.Value,
                () => Theme) });
        }
    }
}
