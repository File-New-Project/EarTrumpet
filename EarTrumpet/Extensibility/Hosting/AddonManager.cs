using EarTrumpet.Extensions;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Linq;

namespace EarTrumpet.Extensibility.Hosting
{
    public class AddonManager
    {
        public static AddonHost Host { get; } = new AddonHost();

        private static readonly AddonResolver _resolver = new AddonResolver();
        private static readonly Dictionary<DirectoryCatalog, AddonInfo> _addons = new Dictionary<DirectoryCatalog, AddonInfo>();

        public static void Load()
        {
            foreach (var catalog in _resolver.Load(Host))
            {
                var info = LookupAddonInfoFromCatalog(catalog);
                if (info != null)
                {
                    _addons.Add(catalog, info);
                }
                else
                {
                    Trace.WriteLine($"AddonManager Load: No AddonInfo for {catalog.LoadedFiles.FirstOrDefault()}");
                }
            }

            Host.AppLifecycleItems.ForEachNoThrow(x => x.OnApplicationLifecycleEvent(ApplicationLifecycleEvent.Startup));
            Host.AppLifecycleItems.ForEachNoThrow(x => x.OnApplicationLifecycleEvent(ApplicationLifecycleEvent.Startup2));
        }

        public static void Shutdown()
        {
            Trace.WriteLine($"AddonManager Shutdown");
            Host.AppLifecycleItems.ForEachNoThrow(x => x.OnApplicationLifecycleEvent(ApplicationLifecycleEvent.Shutdown));
        }

        public static AddonInfo FindAddonInfoForObject(object addonObject)
        {
            var catalog = _addons.Keys.FirstOrDefault(c => IsObjectFromCatalog(addonObject, c));
            if (catalog != null)
            {
                return _addons[catalog];
            }
            return null;
        }

        private static bool IsObjectFromCatalog(object target, DirectoryCatalog catalog)
        {
            var assemblyLocation = target.GetType().Assembly.Location.ToLower();
            return catalog.LoadedFiles.Any(file => file.ToLower() == assemblyLocation);
        }

        private static AddonInfo LookupAddonInfoFromCatalog(DirectoryCatalog catalog)
        {
            return Host.AppLifecycleItems.FirstOrDefault(a => IsObjectFromCatalog(a, catalog))?.Info;
        }

        public static string GetDiagnosticInfo() => string.Join(" ", _addons.Values.Select(a => a.DisplayName));
    }
}