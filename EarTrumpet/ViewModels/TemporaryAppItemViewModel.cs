using EarTrumpet.DataModel.Internal.Services;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace EarTrumpet.ViewModels
{
    class TemporaryAppItemViewModel : IAppItemViewModel
    {
        public string Id { get; }
        public bool IsMuted { get; set; }
        public int Volume { get; set; }
        public SolidColorBrush Background { get; }
        public ObservableCollection<IAppItemViewModel> ChildApps { get; }
        public string DisplayName { get; }
        public string ExeName { get; }
        public string AppId { get; }
        public ImageSource Icon { get; }
        public char IconText { get; }
        public bool IsExpanded { get; }
        public bool IsMovable { get; }
        public float PeakValue { get; }
        public string PersistedOutputDevice => AudioPolicyConfigService.GetDefaultEndPoint(ProcessId);
        public int ProcessId { get; }

        public TemporaryAppItemViewModel(IAppItemViewModel app, bool isChild = false)
        {
            if (!isChild)
            {
                ChildApps = new ObservableCollection<IAppItemViewModel> { new TemporaryAppItemViewModel(app, true) };
            }

            Id = app.Id;
            IsMuted = app.IsMuted;
            Volume = app.Volume;
            Background = app.Background;
            DisplayName = app.DisplayName;
            ExeName = app.ExeName;
            AppId = app.AppId;
            Icon = app.Icon;
            IconText = app.IconText;
            IsMovable = app.IsMovable;
            IsExpanded = isChild;
            PeakValue = 0;
            ProcessId = app.ProcessId;

#if VSDEBUG
            Background = new SolidColorBrush(Colors.Red);
#endif
        }

        public bool DoesGroupWith(IAppItemViewModel app)
        {
            return ExeName == app.ExeName;
        }

        public void MoveAllSessionsToDevice(string id) { }
        public void RefreshDisplayName() { }
        public void UpdatePeakValueBackground() { }
        public void UpdatePeakValueForeground() { }
    }
}
