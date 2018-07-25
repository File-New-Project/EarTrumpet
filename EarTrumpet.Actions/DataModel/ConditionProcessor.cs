using EarTrumpet_Actions.DataModel.Conditions;
using System;

namespace EarTrumpet_Actions.DataModel
{
    class ConditionProcessor
    {
        public static bool IsMet(BaseCondition condition)
        {
            if (condition is ProcessCondition)
            {
                var cond = (ProcessCondition)condition;

                bool ret = Addon.Current.ProcessWatcher.ProcessNames.Contains(cond.Text);

                switch (cond.ConditionType)
                {
                    case ProcessConditionType.IsRunning:
                        return ret;
                    case ProcessConditionType.IsNotRunning:
                        return !ret;
                    default:
                        throw new NotImplementedException();
                }
            }
            else if (condition is DefaultPlaybackDeviceCondition)
            {
                var cond = (DefaultPlaybackDeviceCondition)condition;
                var ret = cond.Device.Id == PlaybackDataModelHost.DeviceManager.Default?.Id;
                switch (cond.Operation)
                {
                    case ComparisonOperation.Is:
                        return ret;
                    case ComparisonOperation.IsNot:
                        return !ret;
                    default:
                        throw new NotImplementedException();
                }
            }

            throw new NotImplementedException();
        }
    }
}
