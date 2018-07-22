using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;

namespace EarTrumpet.Extensibility.Hosting
{
    class AddonHostService
    {
        public static AddonHostService Instance { get; private set; }
        public static void Initialize() => Instance = new AddonHostService();

        [ImportMany(typeof(IApplicationLifecycle))]
        private List<IApplicationLifecycle> _appLifecycle { get; set; }

        [ImportMany(typeof(ISettingsEntry))]
        public List<ISettingsEntry> EntryPoints { get; set; }

        [ImportMany(typeof(IContextMenuItems))]
        public List<IContextMenuItems> ContextMenuItems { get; set; }

        public AddonHostService()
        {
            var cat = new DirectoryCatalog(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "EarTrumpet-*.dll");
            var container = new CompositionContainer(cat);
            container.ComposeParts(this);
        }

        public void OnApplicationLifecycleEvent(ApplicationLifecycleEvent evt)
        {
            _appLifecycle.ToList().ForEach(x => x.OnApplicationLifecycleEvent(evt));
        }

        public void Load()
        {
            OnApplicationLifecycleEvent(ApplicationLifecycleEvent.Startup);
        }

        public void Close()
        {
            OnApplicationLifecycleEvent(ApplicationLifecycleEvent.Shutdown);
        }
    }
}