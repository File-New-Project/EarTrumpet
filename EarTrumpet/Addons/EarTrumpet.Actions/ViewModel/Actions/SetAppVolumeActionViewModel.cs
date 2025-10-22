using EarTrumpet.Actions.DataModel.Enum;
using EarTrumpet.Actions.DataModel.Serialization;
using EarTrumpet.Extensions;
using System;

namespace EarTrumpet.Actions.ViewModel.Actions;

internal class SetAppVolumeActionViewModel : PartViewModel
{
    public OptionViewModel Option { get; }
    public OptionViewModel Unit { get; }
    public DeviceListViewModel Device { get; }
    public AppListViewModel App { get; }
    public VolumeViewModel Volume { get; }

    private SetAppVolumeAction _action;

    public SetAppVolumeActionViewModel(SetAppVolumeAction action) : base(action)
    {
        _action = action;

        Option = new OptionViewModel(action, nameof(action.Option));
        Unit = new OptionViewModel(action, nameof(action.Unit));
        App = new AppListViewModel(action, AppListViewModel.AppKind.EveryApp | AppListViewModel.AppKind.ForegroundApp);
        Device = new DeviceListViewModel(action, DeviceListViewModel.DeviceListKind.DefaultPlayback);
        Volume = new VolumeViewModel(action);

        Attach(Option);
        Attach(Unit);
        Attach(App);
        Attach(Device);
        Attach(Volume);

        Option.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(OptionViewModel.Selected))
            {
                Volume.UpdateRange();
                Volume.Volume = 0;
            }
        };

        Unit.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(OptionViewModel.Selected))
            {
                Volume.UpdateRange();
                Volume.Volume = (VolumeUnit)Unit.Selected.Value switch
                {
                    VolumeUnit.Percentage => Math.Round(Volume.Volume.LogToLinear() * 100),
                    VolumeUnit.Decibel => Math.Round((Volume.Volume / 100).LinearToLog(), 1),
                    _ => throw new ArgumentException("Invalid volume unit."),
                };
            }
        };
    }

    public override string LinkText
    {
        get
        {
            if (_action.Option == SetVolumeKind.Set)
            {
                return base.LinkText;
            }
            else
            {
                return Properties.Resources.SetAppVolumeAction_LinkTextIncrement;
            }
        }
    }
}
