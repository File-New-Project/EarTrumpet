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
            Options = new List<OptionCollection>(new OptionCollection[]{ new OptionCollection(new List<Option>
                {
                new Option(Properties.Resources.ValueComparisonKindIsText, ComparisonBoolKind.Is),
                new Option(Properties.Resources.ValueComparisonKindIsNotText, ComparisonBoolKind.IsNot),
                },
                (newValue) => Option = (ComparisonBoolKind)newValue.Value,
                () => Option) });
        }

        public override string Describe() => Properties.Resources.DefaultDeviceConditionDescribeFormatText;
    }
}
