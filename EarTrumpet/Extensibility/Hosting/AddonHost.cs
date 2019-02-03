using EarTrumpet.Extensions;
using EarTrumpet.UI.ViewModels;
using System;
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
        public class Entry
        {
            public bool IsThirdParty;
            public DirectoryCatalog Catalog;
            public AddonInfo Info;
        }

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

        [ImportMany(typeof(IAddonSettingsPage))]
        private List<IAddonSettingsPage> _settingsPages { get; set; }

        private AddonInfo FindInfo(DirectoryCatalog catalog)
        {
            foreach (var file in catalog.LoadedFiles)
            {
                foreach (var a in _appLifecycle)
                {
                    var asmLocation = a.GetType().Assembly.Location;
                    if (asmLocation.ToLower() == file.ToLower())
                    {
                        return a.Info;
                    }
                }
            }
            return null;
        }

        public IEnumerable<Entry> Initialize(string[] additionalFilePaths)
        {
            var catalogs = new List<Entry>();
            Trace.WriteLine($"AddonHost Initialize");
            try
            {
                if (App.Current.HasIdentity())
                {
                    foreach (var package in Windows.ApplicationModel.Package.Current.Dependencies)
                    {
                        if (package.IsOptional)
                        {
                            Trace.WriteLine($"AddonHost: Loading from: {package.InstalledLocation.Path}");
                            catalogs.Add(new Entry { IsThirdParty = false, Catalog = new DirectoryCatalog(package.InstalledLocation.Path, "*.dll") });
                        }
                    }
                }
                else
                {
                    catalogs.Add(new Entry { IsThirdParty = false, Catalog = new DirectoryCatalog(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "EarTrumpet-*.dll") });
                }

                foreach (var additional in additionalFilePaths)
                {
                    catalogs.Add(new Entry { IsThirdParty = true, Catalog = new DirectoryCatalog(Path.GetDirectoryName(additional), Path.GetFileName(additional)) });
                }

                var container = new CompositionContainer(new AggregateCatalog(catalogs.Select(c => c.Catalog)));
                container.ComposeParts(this);

                foreach(var cat in catalogs)
                {
                    cat.Info = FindInfo(cat.Catalog);
                }

                TrayViewModel.AddonItems = _contextMenuItems.ToArray();
                FocusedAppItemViewModel.AddonContextMenuItems = _appContextMenuItems.ToArray();
                FocusedAppItemViewModel.AddonContentItems = _appContentItems.ToArray();
                FocusedDeviceViewModel.AddonContextMenuItems = _deviceContextMenuItems.ToArray();
                FocusedDeviceViewModel.AddonContentItems = _deviceContentItems.ToArray();
                SettingsViewModel.AddonItems = _settingsPages.ToArray();

                _appLifecycle.ToList().ForEachNoThrow(x => x.OnApplicationLifecycleEvent(ApplicationLifecycleEvent.Startup));
                _appLifecycle.ToList().ForEachNoThrow(x => x.OnApplicationLifecycleEvent(ApplicationLifecycleEvent.Startup2));
                App.Current.Exit += (_, __) => _appLifecycle.ToList().ForEachNoThrow(x => x.OnApplicationLifecycleEvent(ApplicationLifecycleEvent.Shutdown));
            }
            catch(Exception ex)
            {
                Trace.WriteLine($"AddonHost Initialize: {ex}");
            }
            return catalogs;
        }
    }
}