using System;

namespace EarTrumpet
{
    enum Feature
    {
        Addons = 1,
        DevicePopup = 2,
        SoundSettingsMoSetPageOnTrayIcon = 3,
        TrayIconToolTipHasMuteState = 4,
    }

    class Features
    {
        public static bool IsEnabled(Feature feature)
        {
            switch (feature)
            {
                case Feature.DevicePopup:
                case Feature.Addons:
                case Feature.SoundSettingsMoSetPageOnTrayIcon:
                case Feature.TrayIconToolTipHasMuteState:
#if DEBUG
                    return true;
#else
                    return false;
#endif
                default: throw new NotImplementedException();
            }
        }
    }
}
