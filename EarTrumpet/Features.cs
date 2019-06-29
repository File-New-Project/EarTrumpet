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
    }
}
