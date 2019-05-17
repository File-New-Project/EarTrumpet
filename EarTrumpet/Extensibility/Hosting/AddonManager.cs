using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace EarTrumpet.Extensibility.Hosting
{
    class AddonManager
    {
        public List<Addon> All { get; } = new List<Addon>();

        private AddonHost _host;

        public AddonManager()
        {
            Trace.WriteLine("AddonManager Load");
            _host = new AddonHost();
            All.AddRange(_host.Initialize());
            Trace.WriteLine("AddonManager Loaded");
        }

        public AddonInfo FindAddonForObject(object addonObject)
        {
            var asm = addonObject.GetType().Assembly;
            return All.FirstOrDefault(a => a.IsAssembly(asm))?.Info;
        }

        public void Shutdown()
        {
            _host.Shutdown();
        }
    }
}
