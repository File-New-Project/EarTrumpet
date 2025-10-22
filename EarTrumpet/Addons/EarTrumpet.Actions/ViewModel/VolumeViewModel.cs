using EarTrumpet.Actions.DataModel;
using EarTrumpet.Actions.DataModel.Enum;
using System;

namespace EarTrumpet.Actions.ViewModel;

public class VolumeViewModel : BindableBase
{
    public double Volume
    {
        get => _part.Volume;
        set
        {
            _part.Volume = Math.Round(value, _part.Unit switch
            {
                VolumeUnit.Percentage => 0,
                VolumeUnit.Decibel => 1,
                _ => throw new InvalidOperationException("Invalid volume unit."),
            });
            RaisePropertyChanged(nameof(Volume));
        }
    }

    public double Maximum => _part.Unit switch
    {
        VolumeUnit.Percentage => 100,
        VolumeUnit.Decibel => _part.Option switch
        {
            SetVolumeKind.Set => 0,
            SetVolumeKind.Increment => -App.Settings.LogarithmicVolumeMinDb,
            SetVolumeKind.Decrement => -App.Settings.LogarithmicVolumeMinDb,
            _ => throw new InvalidOperationException("Invalid action."),
        },
        _ => throw new InvalidOperationException("Invalid volume unit."),
    };
    public double Minimum => _part.Unit switch
    {
        VolumeUnit.Percentage => 0,
        VolumeUnit.Decibel => _part.Option switch
        {
            SetVolumeKind.Set => App.Settings.LogarithmicVolumeMinDb,
            SetVolumeKind.Increment => 0,
            SetVolumeKind.Decrement => 0,
            _ => throw new InvalidOperationException("Invalid action."),
        },
        _ => throw new InvalidOperationException("Invalid volume unit."),
    };

    private IPartWithVolume _part;
    public VolumeViewModel(IPartWithVolume part)
    {
        _part = part;
    }

    public override string ToString()
    {
        return $"{Volume}";
    }

    public void UpdateRange()
    {
        RaisePropertyChanged(nameof(Maximum));
        RaisePropertyChanged(nameof(Minimum));
    }
}
