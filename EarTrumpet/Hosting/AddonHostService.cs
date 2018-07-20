using EarTrumpet.DataModel;
using EarTrumpet.Extensibility;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;

namespace EarTrumpet.Hosting
{
    class AddonHostService : IApplicationLifecycle
    {
        public static AddonHostService Instance { get; private set; }
        public static void Initialize() => Instance = new AddonHostService();

        [ImportMany(typeof(IApplicationLifecycle))]
        private List<IApplicationLifecycle> _appLifecycle { get; set; }

        [ImportMany(typeof(IPlaybackDevicesDataModel))]
        private List<IPlaybackDevicesDataModel> _playbackDataModel { get; set; }

        [ImportMany(typeof(IHaveSettings))]
        private List<IHaveSettings> _entryPoints { get; set; }

        [ImportMany(typeof(IContextMenuItems))]
        private List<IContextMenuItems> _contextMenuItems { get; set; }

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

        public void InitializePlaybackDataModel(IAudioDeviceManager manager)
        {
            _playbackDataModel.ForEach(x => x.InitializeDataModel(manager));
        }

        public IEnumerable<IHaveSettings> GetEntryPoints()
        {
            return _entryPoints;
        }

        public IEnumerable<IContextMenuItems> GetContextMenuExtensions()
        {
            return _contextMenuItems;
        }
    }
}
