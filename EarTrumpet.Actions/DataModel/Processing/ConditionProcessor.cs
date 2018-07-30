using EarTrumpet.DataModel;
using EarTrumpet_Actions.DataModel.Conditions;
using System;

namespace EarTrumpet_Actions.DataModel.Processing
{
    class ConditionProcessor
    {
        public static bool IsMet(BaseCondition condition)
        {
            if (condition is ProcessCondition)
            {
                bool isProcessRunning = ProcessWatcher.Current.ProcessNames.Contains(((ProcessCondition)condition).Text);
                switch (((ProcessCondition)condition).Option)
                {
                    case ProcessStateKind.Running:
                        return isProcessRunning;
                    case ProcessStateKind.NotRunning:
                        return !isProcessRunning;
                    default:
                        throw new NotImplementedException();
                }
            }
            else if (condition is DefaultDeviceCondition)
            {
                var isDeviceCurrentlyDefault = ((DefaultDeviceCondition)condition).Device.Id == DataModelFactory.CreateAudioDeviceManager(AudioDeviceKind.Playback).Default?.Id;
                switch (((DefaultDeviceCondition)condition).Option)
                {
                    case ValueComparisonKind.Is:
                        return isDeviceCurrentlyDefault;
                    case ValueComparisonKind.IsNot:
                        return !isDeviceCurrentlyDefault;
                    default:
                        throw new NotImplementedException();
                }
            }
            else if (condition is VariableCondition)
            {
                return (Addon.Current.LocalVariables[((VariableCondition)condition).Text] == ((VariableCondition)condition).Value);
            }
            throw new NotImplementedException();
        }
    }
}
