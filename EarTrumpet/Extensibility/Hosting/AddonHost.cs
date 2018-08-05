using EarTrumpet.Extensions;
using EarTrumpet.UI.ViewModels;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
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
        private List<IAddonContextMenu> _contextMenuItems { get; set; }

        [ImportMany(typeof(IAddonAppContent))]
        private List<IAddonAppContent> _appContentItems { get; set; }

        [ImportMany(typeof(IAddonAppContextMenu))]
        private List<IAddonAppContextMenu> _appContextMenuItems { get; set; }

        [ImportMany(typeof(IAddonDeviceContent))]
        private List<IAddonDeviceContent> _deviceContentItems { get; set; }

        [ImportMany(typeof(IAddonDeviceContextMenu))]
        private List<IAddonDeviceContextMenu> _deviceContextMenuItems { get; set; }

        public string[] Initialize(string[] additionalFilePaths)
        {
            var catalogs = new List<ComposablePartCatalog>();

            if (App.Current.HasIdentity())
            {
                foreach (var package in Windows.ApplicationModel.Package.Current.Dependencies)
                {
                    if (package.IsOptional)
                    {
                        Trace.WriteLine($"AddonHost: Loading from: {package.InstalledLocation.Path}");
                        catalogs.Add(new DirectoryCatalog(package.InstalledLocation.Path, "*.dll"));
                    }
                }
            }
            else
            {
                catalogs.Add(new DirectoryCatalog(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "EarTrumpet-*.dll"));
            }

            foreach(var additional in additionalFilePaths)
            {
                catalogs.Add(new DirectoryCatalog(Path.GetDirectoryName(additional), Path.GetFileName(additional)));
            }

            var container = new CompositionContainer(new AggregateCatalog(catalogs));
            container.ComposeParts(this);

            TrayViewModel.AddonItems = _contextMenuItems.ToArray();
            FocusedAppItemViewModel.AddonContextMenuItems = _appContextMenuItems.ToArray();
            FocusedAppItemViewModel.AddonContentItems = _appContentItems.ToArray();
            FocusedDeviceViewModel.AddonContextMenuItems = _deviceContextMenuItems.ToArray();
            FocusedDeviceViewModel.AddonContentItems = _deviceContentItems.ToArray();

            _appLifecycle.ToList().ForEach(x => x.OnApplicationLifecycleEvent(ApplicationLifecycleEvent.Startup));
            _appLifecycle.ToList().ForEach(x => x.OnApplicationLifecycleEvent(ApplicationLifecycleEvent.Startup2));
            App.Current.Exit += (_, __) => _appLifecycle.ToList().ForEach(x => x.OnApplicationLifecycleEvent(ApplicationLifecycleEvent.Shutdown));

            return catalogs.SelectMany(a => ((DirectoryCatalog)a).LoadedFiles).ToArray();
        }
    }
}