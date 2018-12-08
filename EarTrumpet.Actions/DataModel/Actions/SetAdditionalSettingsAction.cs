using EarTrumpet.Extensibility;
using EarTrumpet.Extensibility.Shared;
using EarTrumpet_Actions.DataModel.Enum;
using System.Collections.Generic;
using System.Linq;

namespace EarTrumpet_Actions.DataModel.Actions
{
    public class SetAdditionalSettingsAction : BaseAction
    {
        public ToggleBoolKind Value { get; set; }

        public string SettingId { get; set; }
        
        public override string Describe() => string.Format(Properties.Resources.SetAdditionalSettingsActionDescribeFormatText, Options[0].DisplayName, Options[1].DisplayName);

        public SetAdditionalSettingsAction()
        {
            Description = Properties.Resources.SetAdditionalSettingsActionDescriptionText;
            Options = new List<OptionCollection>(new OptionCollection[]
            {
                new OptionCollection(ServiceBus.GetMany(KnownServices.BoolValue).Select(
                    a => (IValue<bool>)a).Select(
                    v => new Option(v.DisplayName, v.Id)),
                (v) => SettingId = (string)v.Value,
                () => SettingId),
                new OptionCollection(
                    new List<Option>(new Option[]
                    {
                        new Option(Properties.Resources.BoolTrueText, ToggleBoolKind.True),
                        new Option(Properties.Resources.BoolFalseText, ToggleBoolKind.False),
                        new Option(Properties.Resources.ToggleText, ToggleBoolKind.Toggle),
                    }),
                (v) => Value = (ToggleBoolKind)v.Value,
                () => Value)
            });
        }
    }
}
