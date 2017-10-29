using System;
using System.Threading.Tasks;
using Windows.System;

namespace EarTrumpet.Services
{
    public static class FirstRunDisplayService
    {
        internal static void ShowIfAppropriate()
        {
            bool hasShownFirstRun = false;
            if (!Windows.Storage.ApplicationData.Current.LocalSettings.Values.ContainsKey(nameof(hasShownFirstRun)))
            {
                Windows.Storage.ApplicationData.Current.LocalSettings.Values[nameof(hasShownFirstRun)] = true;
                try
                {
                    System.Diagnostics.Process.Start("eartrumpet://welcome");
                }
                catch { }
            }
        }
    }
}
