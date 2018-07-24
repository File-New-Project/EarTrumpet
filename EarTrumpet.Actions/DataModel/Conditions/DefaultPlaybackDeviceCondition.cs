using System;
using System.Collections.Generic;
using System.Linq;

namespace EarTrumpet_Actions.DataModel.Conditions
{
    public enum ComparisonOperation
    {
        Is,
        IsNot,
    }

    public class DefaultPlaybackDeviceCondition : BaseCondition
    {
        public Device Device { get; set; }
        public ComparisonOperation Operation { get; set; }
        
        public DefaultPlaybackDeviceCondition()
        {
            DisplayName = "If the default playback device (is, is not)";
            Options = new List<Option>
            {
                new Option("is", ComparisonOperation.Is),
                new Option("is not", ComparisonOperation.IsNot),
            };
        }

        public override void Loaded()
        {
            var selected = Options.First(o => (ComparisonOperation)o.Value == Operation);
            Option = selected.Value;

            DisplayName = $"The default playback device {Option} {Device}";
        }

        public override bool IsMet()
        {
            var ret = Device.Id == PlaybackDataModelHost.DeviceManager.Default?.Id;
            switch (Operation)
            {
                case ComparisonOperation.Is:
                    return ret;
                case ComparisonOperation.IsNot:
                    return !ret;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
