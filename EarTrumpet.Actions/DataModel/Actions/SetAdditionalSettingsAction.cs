using EarTrumpet.Extensibility;
using EarTrumpet.Extensibility.Shared;
using System.Collections.Generic;
using System.Linq;

namespace EarTrumpet_Actions.DataModel.Actions
{
    public class SetAdditionalSettingsAction : BaseAction
    {
        public bool Value { get; set; }

        public string SettingId { get; set; }

        public override string Describe() => $"Set '{Options[0].DisplayName}' to {Options[1].DisplayName}";

        public SetAdditionalSettingsAction()
        {
            Description = "Set additional EarTrumpet settings";

            Options = new List<OptionData>(new OptionData[]
            {
                new OptionData(ServiceBus.GetMany(KnownServices.ValueService).Where(
                    a => a is IValue<bool>).Select(
                    a => (IValue<bool>)a).Select(
                    v => new Option(v.DisplayName, v.Id)),
                (v) => SettingId = (string)v.Value,
                () => SettingId),
                new OptionData(
                    new List<Option>(new Option[]
                    {
                        new Option("on", true),
                        new Option("off", false),
                    }),
                (v) => Value = (bool)v.Value,
                () => Value)
            });
        }
    }
}
