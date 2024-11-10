using System;
using System.Collections.ObjectModel;

namespace EarTrumpet.Actions.DataModel.Serialization;

public class EarTrumpetAction
{
    public string DisplayName { get; set; }
    public Guid Id { get; set; } = Guid.NewGuid();
    public ObservableCollection<BaseTrigger> Triggers { get; set; } = [];
    public ObservableCollection<BaseCondition> Conditions { get; set; } = [];
    public ObservableCollection<BaseAction> Actions { get; set; } = [];
}
