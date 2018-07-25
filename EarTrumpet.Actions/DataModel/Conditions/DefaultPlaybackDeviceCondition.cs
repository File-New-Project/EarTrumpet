using System;
using System.Collections.Generic;

namespace EarTrumpet_Actions.DataModel.Conditions
{
    public enum ComparisonOperation
    {
        Is,
        IsNot,
    }

    public class DefaultPlaybackDeviceCondition : BaseCondition, IPartWithDevice
    {
        public Device Device { get; set; }
        public ComparisonOperation Operation { get; set; }

        public DefaultPlaybackDeviceCondition()
        {
            Description = "If the default playback device (is, is not)";
            Options = new List<OptionData>(new OptionData[]{ new OptionData(new List<Option>
                {
                new Option("is", ComparisonOperation.Is),
                new Option("is not", ComparisonOperation.IsNot),
                },
                (newValue) => Operation = (ComparisonOperation)newValue.Value,
                () => Operation) });
        }

        public override string Describe() => $"The default playback device {Options[0].DisplayName} {Device}";
    }
}
