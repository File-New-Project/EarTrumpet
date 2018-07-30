using EarTrumpet.DataModel;
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

                bool ret = ProcessWatcher.Current.ProcessNames.Contains(cond.Text);

                switch (cond.Option)
                {
                    case ProcessStateKind.Running:
                        return ret;
                    case ProcessStateKind.NotRunning:
                        return !ret;
                    default:
                        throw new NotImplementedException();
                }
            }
            else if (condition is DefaultPlaybackDeviceCondition)
            {
                var cond = (DefaultPlaybackDeviceCondition)condition;
                var ret = cond.Device.Id == DataModelFactory.CreateAudioDeviceManager(AudioDeviceKind.Playback).Default?.Id;
                switch (cond.Option)
                {
                    case ValueComparisonKind.Is:
                        return ret;
                    case ValueComparisonKind.IsNot:
                        return !ret;
                    default:
                        throw new NotImplementedException();
                }
            }
            else if (condition is VariableCondition)
            {
                var cond = (VariableCondition)condition;
                return (Addon.Current.LocalVariables[cond.Text] == cond.Value);
            }
            throw new NotImplementedException();
        }
    }
}
