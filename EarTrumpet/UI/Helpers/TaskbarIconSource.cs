using EarTrumpet.DataModel;
using EarTrumpet.Interop;
using EarTrumpet.Interop.Helpers;
using EarTrumpet.UI.ViewModels;
using System;
using System.Diagnostics;
using System.Drawing;

namespace EarTrumpet.UI.Helpers;

public class TaskbarIconSource : IShellNotifyIconSource
{
    private enum IconKind
    {
        EarTrumpet,
        Muted,
        SpeakerZeroBars,
        SpeakerOneBar,
        SpeakerTwoBars,
        SpeakerThreeBars,
        NoDevice,
    }

    public event Action<IShellNotifyIconSource> Changed;

    public Icon Current { get; private set; }
    public bool IsWhiteIcon { get => true; }

    private readonly DeviceCollectionViewModel _collection;
    private readonly AppSettings _settings;
    private bool _isMouseOver;
    private string _hash;
    private IconKind _kind;

    public TaskbarIconSource(DeviceCollectionViewModel collection, AppSettings settings)
    {
        _collection = collection;
        _settings = settings;

        _settings.UseLegacyIconChanged += (_, __) => CheckForUpdate();
        collection.TrayPropertyChanged += OnTrayPropertyChanged;

        OnTrayPropertyChanged();
    }

    public void OnMouseOverChanged(bool isMouseOver)
    {
        _isMouseOver = isMouseOver;
        CheckForUpdate();
    }

    public void CheckForUpdate()
    {
        var nextHash = GetHash();
        if (nextHash != _hash)
        {
            Trace.WriteLine($"TaskbarIconSource Changed: {nextHash}");
            _hash = nextHash;
            using (var old = Current)
            {
                Current = SelectAndLoadIcon(_kind);
                Changed?.Invoke(this);
            }
        }
    }

    private void OnTrayPropertyChanged()
    {
        _kind = IconKindFromDeviceCollection(_collection);
        CheckForUpdate();
    }

    private Icon SelectAndLoadIcon(IconKind kind)
    {
        if (_settings.UseLegacyIcon)
        {
            kind = IconKind.EarTrumpet;
        }

        try
        {
            return LoadIcon(kind);
        }
        // Legacy fallback if SndVolSSD.dll icons are unavailable.
        catch (Exception ex) when (kind != IconKind.EarTrumpet)
        {
            Trace.WriteLine($"TaskbarIconSource LoadIcon: {ex}");
            return SelectAndLoadIcon(IconKind.EarTrumpet);
        }
    }

    private static Icon LoadIcon(IconKind kind)
    {
        var dpi = WindowsTaskbar.Dpi;
        switch (kind)
        {
            case IconKind.EarTrumpet:
                return IconHelper.LoadIconForTaskbar((string)App.Current.Resources["EarTrumpetIconDark"], dpi);
            case IconKind.Muted:
                return IconHelper.LoadIconForTaskbar(SndVolSSO.GetPath(SndVolSSO.IconId.Muted), dpi);
            case IconKind.NoDevice:
                return IconHelper.LoadIconForTaskbar(SndVolSSO.GetPath(SndVolSSO.IconId.NoDevice), dpi);
            case IconKind.SpeakerZeroBars:
                return IconHelper.LoadIconForTaskbar(SndVolSSO.GetPath(SndVolSSO.IconId.SpeakerZeroBars), dpi);
            case IconKind.SpeakerOneBar:
                return IconHelper.LoadIconForTaskbar(SndVolSSO.GetPath(SndVolSSO.IconId.SpeakerOneBar), dpi);
            case IconKind.SpeakerTwoBars:
                return IconHelper.LoadIconForTaskbar(SndVolSSO.GetPath(SndVolSSO.IconId.SpeakerTwoBars), dpi);
            case IconKind.SpeakerThreeBars:
                return IconHelper.LoadIconForTaskbar(SndVolSSO.GetPath(SndVolSSO.IconId.SpeakerThreeBars), dpi);
            default: throw new NotImplementedException();
        }
    }

    private string GetHash() =>
        $"kind={_kind} " +
        $"dpi={WindowsTaskbar.Dpi} " +
        $"isLegacy={_settings.UseLegacyIcon}";

    private static IconKind IconKindFromDeviceCollection(DeviceCollectionViewModel collectionViewModel)
    {
        if (collectionViewModel.Default != null)
        {
            switch (collectionViewModel.Default.IconKind)
            {
                case DeviceViewModel.DeviceIconKind.Mute:
                    return IconKind.Muted;
                case DeviceViewModel.DeviceIconKind.Bar0:
                    return IconKind.SpeakerZeroBars;
                case DeviceViewModel.DeviceIconKind.Bar1:
                    return IconKind.SpeakerOneBar;
                case DeviceViewModel.DeviceIconKind.Bar2:
                    return IconKind.SpeakerTwoBars;
                case DeviceViewModel.DeviceIconKind.Bar3:
                    return IconKind.SpeakerThreeBars;
                default: throw new NotImplementedException();
            }
        }
        return IconKind.NoDevice;
    }
}
