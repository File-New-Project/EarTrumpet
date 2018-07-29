using EarTrumpet.UI.ViewModels;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Linq;
using System.Reflection;

namespace EarTrumpet.Extensibility.Hosting
{
    class AddonHost
    {
        [ImportMany(typeof(IAddonLifecycle))]
        private List<IAddonLifecycle> _appLifecycle { get; set; }

        [ImportMany(typeof(IAddonContextMenu))]
        public List<IAddonContextMenu> _contextMenuItems { get; set; }

        [ImportMany(typeof(IAddonAppContextMenu))]
        public List<IAddonAppContextMenu> _appContextMenuItems { get; set; }

        public string[] Initialize(string[] additionals)
        {
            var catalogs = new List<ComposablePartCatalog>();
            catalogs.Add(new DirectoryCatalog(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "EarTrumpet-*.dll"));
            
            foreach(var additional in additionals)
            {
                catalogs.Add(new DirectoryCatalog(Path.GetDirectoryName(additional), Path.GetFileName(additional)));
            }

            var container = new CompositionContainer(new AggregateCatalog(catalogs));
            container.ComposeParts(this);

            ((App)App.Current).TrayViewModel.AddonItems = _contextMenuItems.ToArray();
            FocusedAppItemViewModel.AddonItems = _appContextMenuItems.ToArray();

            _appLifecycle.ToList().ForEach(x => x.OnApplicationLifecycleEvent(ApplicationLifecycleEvent.Startup));
            _appLifecycle.ToList().ForEach(x => x.OnApplicationLifecycleEvent(ApplicationLifecycleEvent.Startup2));
            App.Current.Exit += (_, __) => _appLifecycle.ToList().ForEach(x => x.OnApplicationLifecycleEvent(ApplicationLifecycleEvent.Shutdown));

            return catalogs.SelectMany(a => ((DirectoryCatalog)a).LoadedFiles).ToArray();
        }
    }
}