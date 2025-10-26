using EarTrumpet.Actions.DataModel.Enum;
using EarTrumpet.Actions.DataModel.Serialization;
using EarTrumpet.Extensions;
using System;

namespace EarTrumpet.Actions.ViewModel.Actions;

internal class SetDeviceVolumeActionViewModel : PartViewModel
{
    public OptionViewModel Option { get; }
    public OptionViewModel Unit { get; }
    public DeviceListViewModel Device { get; }
    public VolumeViewModel Volume { get; }

    private SetDeviceVolumeAction _action;

    public SetDeviceVolumeActionViewModel(SetDeviceVolumeAction action) : base(action)
    {
        _action = action;
        Option = new OptionViewModel(action, nameof(action.Option));
        Unit = new OptionViewModel(action, nameof(action.Unit));
        Device = new DeviceListViewModel(action, DeviceListViewModel.DeviceListKind.Recording | DeviceListViewModel.DeviceListKind.DefaultPlayback);
        Volume = new VolumeViewModel(action);

        Attach(Option);
        Attach(Unit);
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
                    VolumeUnit.Percentage => 100,
                    VolumeUnit.Decibel => 0,
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
                return Properties.Resources.SetDeviceVolumeAction_LinkTextIncrement;
            }
        }
    }
}
