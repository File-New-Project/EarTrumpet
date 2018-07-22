using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;

namespace EarTrumpet.Extensibility.Hosting
{
    class AddonHost
    {
        public static AddonHost Current { get; } = new AddonHost();

        [ImportMany(typeof(IAddonLifecycle))]
        private List<IAddonLifecycle> _appLifecycle { get; set; }

        [ImportMany(typeof(IAddonContextMenu))]
        public List<IAddonContextMenu> ContextMenuItems { get; set; }

        public IEnumerable<string> LoadedAddons { get; }

        public AddonHost()
        {
            var cat = new DirectoryCatalog(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "EarTrumpet-*.dll");
            LoadedAddons = cat.LoadedFiles.Select(f => Path.GetFileName(f));
            var container = new CompositionContainer(cat);
            container.ComposeParts(this);
        }

        public void RaiseEvent(ApplicationLifecycleEvent evt)
        {
            _appLifecycle.ToList().ForEach(x => x.OnApplicationLifecycleEvent(evt));
        }
    }
}