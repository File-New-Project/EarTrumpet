using System.Collections.Generic;
using System.Linq;

namespace EarTrumpet.Extensibility.Hosting
{
    class AddonManager
    {
        public static AddonManager Current { get; } = new AddonManager();
        public List<Addon> All { get; } = new List<Addon>();

        private AddonHost _host;

        public void Load()
        {
            _host = new AddonHost();
            All.AddRange(_host.Initialize());
        }

        public Addon FindAddonForObject(object addonObject)
        {
            var asm = addonObject.GetType().Assembly;
            return All.FirstOrDefault(a => a.IsAssembly(asm));
        }
    }
}
