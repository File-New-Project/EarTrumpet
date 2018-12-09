using EarTrumpet.DataModel;
using EarTrumpet_Actions.DataModel.Conditions;
using EarTrumpet_Actions.DataModel.Enum;
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
                var mgr = DataModelFactory.CreateAudioDeviceManager(((DefaultDeviceCondition)condition).Device.Kind);

                var isDeviceCurrentlyDefault = ((DefaultDeviceCondition)condition).Device.Id == mgr.Default?.Id;
                switch (((DefaultDeviceCondition)condition).Option)
                {
                    case ComparisonBoolKind.Is:
                        return isDeviceCurrentlyDefault;
                    case ComparisonBoolKind.IsNot:
                        return !isDeviceCurrentlyDefault;
                    default:
                        throw new NotImplementedException();
                }
            }
            else if (condition is VariableCondition)
            {
                return (Addon.Current.LocalVariables[((VariableCondition)condition).Text] == (((VariableCondition)condition).Value == BoolValue.True));
            }
            throw new NotImplementedException();
        }
    }
}
