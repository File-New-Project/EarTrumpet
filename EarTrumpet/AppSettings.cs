using EarTrumpet.DataModel.Storage;
using EarTrumpet.Interop.Helpers;
using System;
using System.Diagnostics;
using System.Linq;
using static EarTrumpet.Interop.User32;

namespace EarTrumpet;

public class AppSettings
{
    public event EventHandler<bool> UseLegacyIconChanged;
    public event EventHandler<EventArgs> UseLogarithmicVolumeChanged;
    public event Action FlyoutHotkeyTyped;
    public event Action MixerHotkeyTyped;
    public event Action SettingsHotkeyTyped;
    public event Action AbsoluteVolumeUpHotkeyTyped;
    public event Action AbsoluteVolumeDownHotkeyTyped;

    private readonly ISettingsBag _settings = StorageFactory.GetSettings();

    public void RegisterHotkeys()
    {
        HotkeyManager.Current.Register(FlyoutHotkey);
        HotkeyManager.Current.Register(MixerHotkey);
        HotkeyManager.Current.Register(SettingsHotkey);
        HotkeyManager.Current.Register(AbsoluteVolumeUpHotkey);
        HotkeyManager.Current.Register(AbsoluteVolumeDownHotkey);

        HotkeyManager.Current.KeyPressed += (hotkey) =>
        {
            if (hotkey.Equals(FlyoutHotkey))
            {
                Trace.WriteLine("AppSettings FlyoutHotkeyTyped");
                FlyoutHotkeyTyped?.Invoke();
            }
            else if (hotkey.Equals(SettingsHotkey))
            {
                Trace.WriteLine("AppSettings SettingsHotkeyTyped");
                SettingsHotkeyTyped?.Invoke();
            }
            else if (hotkey.Equals(MixerHotkey))
            {
                Trace.WriteLine("AppSettings MixerHotkeyTyped");
                MixerHotkeyTyped?.Invoke();
            }
            else if (hotkey.Equals(AbsoluteVolumeUpHotkey))
            {
                Trace.WriteLine("AppSettings AbsoluteVolumeUpHotkeyTyped");
                AbsoluteVolumeUpHotkeyTyped?.Invoke();
            }
            else if (hotkey.Equals(AbsoluteVolumeDownHotkey))
            {
                Trace.WriteLine("AppSettings AbsoluteVolumeDownHotkeyTyped");
                AbsoluteVolumeDownHotkeyTyped?.Invoke();
            }
        };
    }

    public HotkeyData FlyoutHotkey
    {
        get => _settings.Get("Hotkey", new HotkeyData { });
        set
        {
            HotkeyManager.Current.Unregister(FlyoutHotkey);
            _settings.Set("Hotkey", value);
            HotkeyManager.Current.Register(FlyoutHotkey);
        }
    }

    public HotkeyData MixerHotkey
    {
        get => _settings.Get("MixerHotkey", new HotkeyData { });
        set
        {
            HotkeyManager.Current.Unregister(MixerHotkey);
            _settings.Set("MixerHotkey", value);
            HotkeyManager.Current.Register(MixerHotkey);
        }
    }

    public HotkeyData SettingsHotkey
    {
        get => _settings.Get("SettingsHotkey", new HotkeyData { });
        set
        {
            HotkeyManager.Current.Unregister(SettingsHotkey);
            _settings.Set("SettingsHotkey", value);
            HotkeyManager.Current.Register(SettingsHotkey);
        }
    }

    public HotkeyData AbsoluteVolumeUpHotkey
    {
        get => _settings.Get("AbsoluteVolumeUpHotkey", new HotkeyData { });
        set
        {
            HotkeyManager.Current.Unregister(AbsoluteVolumeUpHotkey);
            _settings.Set("AbsoluteVolumeUpHotkey", value);
            HotkeyManager.Current.Register(AbsoluteVolumeUpHotkey);
        }
    }

    public HotkeyData AbsoluteVolumeDownHotkey
    {
        get => _settings.Get("AbsoluteVolumeDownHotkey", new HotkeyData { });
        set
        {
            HotkeyManager.Current.Unregister(AbsoluteVolumeDownHotkey);
            _settings.Set("AbsoluteVolumeDownHotkey", value);
            HotkeyManager.Current.Register(AbsoluteVolumeDownHotkey);
        }
    }

    public bool UseLegacyIcon
    {
        get
        {
            // Note: Legacy compat, we used to write string bools.
            var ret = _settings.Get("UseLegacyIcon", "False");
            _ = bool.TryParse(ret, out var isUseLegacyIcon);
            return isUseLegacyIcon;
        }
        set
        {
            _settings.Set("UseLegacyIcon", value.ToString());
            UseLegacyIconChanged?.Invoke(null, UseLegacyIcon);
        }
    }

    public bool IsExpanded
    {
        get => _settings.Get("IsExpanded", false);
        set => _settings.Set("IsExpanded", value);
    }

    public bool UseScrollWheelInTray
    {
        get => _settings.Get("UseScrollWheelInTray", true);
        set => _settings.Set("UseScrollWheelInTray", value);
    }

    public bool UseGlobalMouseWheelHook
    {
        get => _settings.Get("UseGlobalMouseWheelHook", false);
        set => _settings.Set("UseGlobalMouseWheelHook", value);
    }

    public bool HasShownFirstRun
    {
        get => _settings.HasKey("hasShownFirstRun");
        set => _settings.Set("hasShownFirstRun", value);
    }

    public bool IsTelemetryEnabled
    {
        get => _settings.Get("IsTelemetryEnabled", IsTelemetryEnabledByDefault());
        set => _settings.Set("IsTelemetryEnabled", value);
    }

    public bool UseLogarithmicVolume
    {
        get => _settings.Get("UseLogarithmicVolume", false);
        set
        {
            _settings.Set("UseLogarithmicVolume", value);
            UseLogarithmicVolumeChanged?.Invoke(this, new EventArgs());
        }
    }

    public float LogarithmicVolumeMinDb
    {
        get => _settings.Get("LogarithmicVolumeMinDb", -40f);
        set
        {
            _settings.Set("LogarithmicVolumeMinDb", value);
            UseLogarithmicVolumeChanged?.Invoke(this, new EventArgs());
        }
    }

    public int VolumeStepAmount
    {
        get => _settings.Get("VolumeStepAmount", 2);
        set => _settings.Set("VolumeStepAmount", Math.Max(1, Math.Min(50, value)));
    }

    public bool UseRangeSnapping
    {
        get => _settings.Get("UseRangeSnapping", true);
        set => _settings.Set("UseRangeSnapping", value);
    }

    public bool UseSliderSnap
    {
        get => _settings.Get("UseSliderSnap", false);
        set => _settings.Set("UseSliderSnap", value);
    }

    public WINDOWPLACEMENT? FullMixerWindowPlacement
    {
        get => _settings.Get("FullMixerWindowPlacement", default(WINDOWPLACEMENT?));
        set => _settings.Set("FullMixerWindowPlacement", value);
    }

    public WINDOWPLACEMENT? SettingsWindowPlacement
    {
        get => _settings.Get("SettingsWindowPlacement", default(WINDOWPLACEMENT?));
        set => _settings.Set("SettingsWindowPlacement", value);
    }

    public bool ShowFullMixerWindowOnStartup
    {
        get => _settings.Get("ShowFullMixerWindowOnStartup", false);
        set => _settings.Set("ShowFullMixerWindowOnStartup", value);
    }

    public Guid TrayIconIdentity
    {
        get
        {
            var id = _settings.Get("TrayIconId", Guid.Empty);
            if (id == Guid.Empty)
            {
                id = Guid.NewGuid();
                _settings.Set("TrayIconId", id);
            }
            return id;
        }
    }

    private static bool IsTelemetryEnabledByDefault()
    {
        // Discussion on what to include:
        // https://gist.github.com/henrik/1688572
        var europeanUnionRegions = new string[]
        {
            // EU 28
            "AT", // Austria
            "BE", // Belgium
            "BG", // Bulgaria
            "HR", // Croatia
            "CY", // Cyprus
            "CZ", // Czech Republic
            "DK", // Denmark
            "EE", // Estonia
            "FI", // Finland
            "FR", // France
            "DE", // Germany
            "GR", // Greece
            "HU", // Hungary
            "IE", // Ireland, Republic of (EIRE)
            "IT", // Italy
            "LV", // Latvia
            "LT", // Lithuania
            "LU", // Luxembourg
            "MT", // Malta
            "NL", // Netherlands
            "PL", // Poland
            "PT", // Portugal
            "RO", // Romania
            "SK", // Slovakia
            "SI", // Slovenia
            "ES", // Spain
            "SE", // Sweden
            "GB", // United Kingdom (Great Britain)

            // Outermost Regions (OMR)
            "GF", // French Guiana
            "GP", // Guadeloupe
            "MQ", // Martinique
            "ME", // Montenegro
            "YT", // Mayotte
            "RE", // Réunion
            "MF", // Saint Martin

            // Special Cases: Part of EU
            "GI", // Gibraltar
            "AX", // Åland Islands

            // Overseas Countries and Territories (OCT)
            "PM", // Saint Pierre and Miquelon
            "GL", // Greenland
            "BL", // Saint Bartelemey
            "SX", // Sint Maarten
            "AW", // Aruba
            "CW", // Curacao
            "WF", // Wallis and Futuna
            "PF", // French Polynesia
            "NC", // New Caledonia
            "TF", // French Southern Territories
            "AI", // Anguilla
            "BM", // Bermuda
            "IO", // British Indian Ocean Territory
            "VG", // Virgin Islands, British
            "KY", // Cayman Islands
            "FK", // Falkland Islands (Malvinas)
            "MS", // Montserrat
            "PN", // Pitcairn
            "SH", // Saint Helena
            "GS", // South Georgia and the South Sandwich Islands
            "TC", // Turks and Caicos Islands

            // Microstates
            "AD", // Andorra
            "LI", // Liechtenstein
            "MC", // Monaco
            "SM", // San Marino
            "VA", // Vatican City

            // Other
            "JE", // Jersey
            "GG", // Guernsey
        };
        var region = new Windows.Globalization.GeographicRegion();
        return !europeanUnionRegions.Contains(region.CodeTwoLetter);
    }
}