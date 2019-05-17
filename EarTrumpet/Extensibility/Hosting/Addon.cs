using System;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reflection;

namespace EarTrumpet.Extensibility.Hosting
{
    public class Addon
    {
        public string DisplayName => _info.DisplayName;
        public string PublisherName => _info.PublisherName;
        public string HelpLink => _info.HelpLink;
        public Version Version => _info.AddonVersion;
        public bool IsValid => _info != null;
        public AddonInfo Info => _info;

        private DirectoryCatalog _catalog;
        private AddonInfo _info;

        public Addon(DirectoryCatalog catalog, AddonInfo info)
        {
            _catalog = catalog;
            _info = info;
        }

        public bool IsAssembly(Assembly asm)
        {
            return _catalog.LoadedFiles.Any(file => file.ToLower() == asm.Location.ToLower());
        }
    }
}
