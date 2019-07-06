using EarTrumpet.DataModel.Audio;
using EarTrumpet.DataModel.WindowsAudio.Internal;
using EarTrumpet.DataModel.WindowsAudio;
using EarTrumpet.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media;

namespace EarTrumpet.UI.ViewModels
{
    public class AppItemViewModel : AudioSessionViewModel, IAppItemViewModel
    {
        public class ExeNameComparer : IComparer<IAppItemViewModel>
        {
            public int Compare(IAppItemViewModel one, IAppItemViewModel two)
            {
                return string.Compare(one.ExeName, two.ExeName, StringComparison.Ordinal);
            }
        }

        public static readonly ExeNameComparer CompareByExeName = new ExeNameComparer();

        public Color Background { get; private set; }
        public char IconText => string.IsNullOrWhiteSpace(DisplayName) ? '?' : DisplayName.ToUpperInvariant().FirstOrDefault(x => char.IsLetterOrDigit(x));
        public string DisplayName => _session.DisplayName;
        public string ExeName => _session.ExeName;
        public string AppId => _session.AppId;
        public string IconPath => _session.IconPath;
        public bool IsDesktopApp => _session.IsDesktopApp;
        public bool IsExpanded { get; private set; }
        public int ProcessId => _session.ProcessId;
        public ObservableCollection<IAppItemViewModel> ChildApps { get; private set; }

        public bool IsMovable => !_session.IsSystemSoundsSession &&
                                  Environment.OSVersion.IsAtLeast(OSVersions.RS4);
        public string PersistedOutputDevice => _session.Parent.Parent is IAudioDeviceManagerWindowsAudio ?
            ((IAudioDeviceManagerWindowsAudio)_session.Parent.Parent).GetDefaultEndPoint(ProcessId) : "";

        public IDeviceViewModel Parent
        {
            get
            {
                if (_parent.TryGetTarget(out var parent))
                {
                    return parent;
                }
                return null;
            }
        }

        private readonly IAudioDeviceSession _session;
        private readonly WeakReference<DeviceViewModel> _parent;

        internal AppItemViewModel(DeviceViewModel parent, IAudioDeviceSession session, bool isChild = false) : base(session)
        {
            IsExpanded = isChild;
            _session = session;
            _session.PropertyChanged += Session_PropertyChanged;
            _parent = new WeakReference<DeviceViewModel>(parent);

            Background = session.IsDesktopApp ? Colors.Transparent : session.BackgroundColor.ToABGRColor();

            if (_session.Children != null)
            {
                _session.Children.CollectionChanged += Children_CollectionChanged;
                ChildApps = new ObservableCollection<IAppItemViewModel>(_session.Children.Select(t => new AppItemViewModel(parent, t, true)));
            }
        }

        ~AppItemViewModel()
        {
            _session.PropertyChanged -= Session_PropertyChanged;
        }

        private void Session_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_session.DisplayName))
            {
                RaisePropertyChanged(nameof(DisplayName));
            }
        }

        private void Children_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_parent.TryGetTarget(out var parent))
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        Debug.Assert(e.NewItems.Count == 1);
                        ChildApps.Add(new AppItemViewModel(parent, (IAudioDeviceSession)e.NewItems[0], true));
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        Debug.Assert(e.OldItems.Count == 1);
                        ChildApps.Remove(ChildApps.First(x => x.Id == ((IAudioDeviceSession)e.OldItems[0]).Id));
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public void MoveToDevice(string id, bool hide)
        {
            ((IAudioDeviceSessionInternal)_session).MoveToDevice(id, hide);
        }

        public override void UpdatePeakValueForeground()
        {
            if (ChildApps != null)
            {
                foreach (var child in ChildApps)
                {
                    child.UpdatePeakValueForeground();
                }
            }

            base.UpdatePeakValueForeground();
        }

        public void UpdatePeakValueBackground()
        {
            if (ChildApps != null)
            {
                foreach (var child in ChildApps.ToArray())
                {
                    child.UpdatePeakValueBackground();
                }
            }

            ((IAudioDeviceSessionInternal)_session).UpdatePeakValueBackground();
        }

        public bool DoesGroupWith(IAppItemViewModel app) => (AppId == app.AppId);

       public override string ToString() => IsMuted ? Properties.Resources.AppOrDeviceMutedFormatAccessibleText.Replace("{Name}", DisplayName) :
            Properties.Resources.AppOrDeviceFormatAccessibleText.Replace("{Name}", DisplayName).Replace("{Volume}", Volume.ToString());
    }
}
