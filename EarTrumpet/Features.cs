namespace EarTrumpet
{
    public class Features
    {
        // Reason:
        //
        // Localization required: ToggleMuteToolTip
        public static bool IsDeviceMuteToolTipEnabled
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }

        // Reason:
        //
        // Localization required: PrivacyCheckboxText
        // Localization required: PrivacyLinkText
        public static bool IsTelemetryConfigurable
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }
    }
}
