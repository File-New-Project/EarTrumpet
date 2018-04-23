using System;
using System.Threading.Tasks;
using Windows.System;

namespace EarTrumpet.Services
{
    public static class FirstRunDisplayService
    {
        internal static void ShowIfAppropriate()
        {
            if (App.HasIdentity())
            {
                bool hasShownFirstRun = false;
                try
                {

                    if (!Windows.Storage.ApplicationData.Current.LocalSettings.Values.ContainsKey(nameof(hasShownFirstRun)))
                    {
                        Windows.Storage.ApplicationData.Current.LocalSettings.Values[nameof(hasShownFirstRun)] = true;
                        try
                        {
                            System.Diagnostics.Process.Start("eartrumpet://welcome");
                        }
                        catch
                        {
                            // In case Process.Start throws, no need to do anything
                        }
                    }
                }
                catch
                {
                    // In case Windows Storage APIs are not stable (seen in Dev Dashboard)
                }
            }
        }
    }
}
