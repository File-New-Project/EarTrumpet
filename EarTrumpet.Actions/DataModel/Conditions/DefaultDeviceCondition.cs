using EarTrumpet_Actions.DataModel.Enum;
using System;
using System.Collections.Generic;

namespace EarTrumpet_Actions.DataModel.Conditions
{
    public class DefaultDeviceCondition : BaseCondition, IPartWithDevice
    {
        public Device Device { get; set; }
        public ComparisonBoolKind Option { get; set; }

        public DefaultDeviceCondition()
        {
            Description = Properties.Resources.DefaultDeviceConditionDescriptionText;
            Options = new List<OptionData>(new OptionData[]{ new OptionData(new List<Option>
                {
                new Option(Properties.Resources.ValueComparisonKindIsText, ComparisonBoolKind.Is),
                new Option(Properties.Resources.ValueComparisonKindIsNotText, ComparisonBoolKind.IsNot),
                },
                (newValue) => Option = (ComparisonBoolKind)newValue.Value,
                () => Option) });
        }

        public override string Describe() => string.Format(Properties.Resources.DefaultDeviceConditionDescribeFormatText, Options[0].DisplayName, Device);
    }
}
