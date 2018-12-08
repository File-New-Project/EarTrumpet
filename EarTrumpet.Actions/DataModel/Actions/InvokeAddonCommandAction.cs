using EarTrumpet.Extensibility;
using EarTrumpet.Extensibility.Shared;
using System.Collections.Generic;
using System.Linq;

namespace EarTrumpet_Actions.DataModel.Actions
{
    public class InvokeAddonCommandAction : BaseAction
    {
        public string OptionId { get; set; }

        public override string Describe() => $"{Options[0]}";

        public InvokeAddonCommandAction()
        {
            var commands = ServiceBus.GetMany(KnownServices.Command).Select(c => (SimpleCommand)c);

            Description = Properties.Resources.InvokeAdditionalCommandsText;
            Options = new List<OptionCollection>(new OptionCollection[]{ new OptionCollection(
                new List<Option>(commands.Select(c => new Option(c.DisplayName, c.Id))),
                (newValue) => OptionId = (string)newValue.Value,
                () => OptionId) });
        }
    }
}
