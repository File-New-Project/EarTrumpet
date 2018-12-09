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
        }

        public override string Describe() => Properties.Resources.DefaultDeviceConditionDescribeFormatText;
    }
}
