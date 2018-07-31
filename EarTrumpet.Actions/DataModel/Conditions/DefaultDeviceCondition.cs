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
            Description = Properties.Resources.DefaultDeviceConditionDescriptionText;
            Options = new List<OptionData>(new OptionData[]{ new OptionData(new List<Option>
                {
                new Option(Properties.Resources.ValueComparisonKindIsText, ValueComparisonKind.Is),
                new Option(Properties.Resources.ValueComparisonKindIsNotText, ValueComparisonKind.IsNot),
                },
                (newValue) => Option = (ValueComparisonKind)newValue.Value,
                () => Option) });
        }

        public override string Describe() => $"The default playback device {Options[0].DisplayName} {Device}";
    }
}
