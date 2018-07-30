using EarTrumpet.Extensibility;
using EarTrumpet.Extensibility.Shared;
using System.Collections.Generic;
using System.Linq;

namespace EarTrumpet_Actions.DataModel.Actions
{
    public class SetAddonEarTrumpetSettingsAction : BaseAction
    {
        public bool Value { get; set; }

        public string Option { get; set; }


        public override string Describe() => $"Set '{Options[0].DisplayName}' to {Options[1].DisplayName}";

        public SetAddonEarTrumpetSettingsAction()
        {
            Description = "Set additional EarTrumpet settings";

            var boolOpt = new OptionData(
                new List<Option>(new Option[]
                {
                    new Option("on", true),
                    new Option("off", false),
                }),
                (v) => Value = (bool)v.Value,
                () => Value);

            var values = new List<IValue<bool>>();
            var addonValues = ServiceBus.GetMany(KnownServices.ValueService);
            if (addonValues != null)
            {
                values = addonValues.Where(a => a is IValue<bool>).Select(a => (IValue<bool>)a).ToList();
            }

            var optionOpt = new OptionData(values.Select(v => new Option(v.DisplayName, v.Id)),
            (v) => Option = (string)v.Value,
            () => Option);

            if (Option == null)
            {
                Option = (string)optionOpt.Options.First().Value;
            }

            Options = new List<OptionData>(new OptionData[]
            {
                optionOpt,
                boolOpt
            });
        }
    }
}
