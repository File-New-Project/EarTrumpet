using EarTrumpet.Extensions;

namespace EarTrumpet.Services
{
    static class FirstRunDisplayService
    {
        internal static void ShowIfAppropriate()
        {
            if (App.Current.HasIdentity())
            {
                bool hasShownFirstRun = false;
                if (!Windows.Storage.ApplicationData.Current.LocalSettings.Values.ContainsKey(nameof(hasShownFirstRun)))
                {
                    Windows.Storage.ApplicationData.Current.LocalSettings.Values[nameof(hasShownFirstRun)] = true;
                    try
                    {
                        var handle = System.Diagnostics.Process.Start("eartrumpet://welcome");
                        App.Current.Exit += (_, __) =>
                        {
                            try
                            {
                                handle.Kill();
                            }
                            catch { }
                        };
                    }
                    catch { }
                }
            }
        }
    }
}
