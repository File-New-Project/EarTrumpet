using System;
using System.Collections.Generic;

namespace EarTrumpet_Actions.DataModel.Conditions
{
    public enum ValueComparisonKind
    {
        Is,
        IsNot,
    }

    public class DefaultDeviceCondition : BaseCondition, IPartWithDevice
    {
        public Device Device { get; set; }
        public ValueComparisonKind Option { get; set; }

        public DefaultDeviceCondition()
        {
            Description = "If the default playback device (is, is not)";
            Options = new List<OptionData>(new OptionData[]{ new OptionData(new List<Option>
                {
                new Option("is", ValueComparisonKind.Is),
                new Option("is not", ValueComparisonKind.IsNot),
                },
                (newValue) => Option = (ValueComparisonKind)newValue.Value,
                () => Option) });
        }

        public override string Describe() => $"The default playback device {Options[0].DisplayName} {Device}";
    }
}
