using EarTrumpet.DataModel;
using EarTrumpet.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EarTrumpet.ViewModels
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

        public ImageSource Icon { get; private set; }

        public SolidColorBrush Background { get; private set; }

        public char IconText => string.IsNullOrWhiteSpace(DisplayName) ? '?' : DisplayName.ToUpperInvariant().FirstOrDefault(x => char.IsLetterOrDigit(x));

        public string DisplayName => _session.DisplayName;

        public string ExeName => _session.ExeName;
        public string AppId => _session.AppId;

        public ObservableCollection<IAppItemViewModel> ChildApps { get; private set; }

        public bool IsMovable => !_session.IsSystemSoundsSession;

        public string PersistedOutputDevice => _session.PersistedDefaultEndPointId;

        public bool IsExpanded { get; private set; }

        public int ProcessId => _session.ProcessId;

        private IAudioDeviceSession _session;

        internal AppItemViewModel(IAudioDeviceSession session, bool isChild = false) : base(session)
        {
            IsExpanded = isChild;
            _session = session;
            _session.PropertyChanged += Session_PropertyChanged;

            if (_session.Children != null)
            {
                _session.Children.CollectionChanged += Children_CollectionChanged;
                ChildApps = new ObservableCollection<IAppItemViewModel>(_session.Children.Select(t => new AppItemViewModel(t, isChild: true)));
            }

            Background = new SolidColorBrush(session.IsDesktopApp ? Colors.Transparent : session.BackgroundColor.ToABGRColor());

            try
            {
                if (session.IsDesktopApp)
                {
                    Icon = System.Drawing.Icon.ExtractAssociatedIcon(session.IconPath).ToImageSource();
                }
                else
                {
                    Icon = new BitmapImage(new Uri(session.IconPath));
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Failed to load icon: {ex}");
            }
        }

        ~AppItemViewModel()
        {
            _session.PropertyChanged -= Session_PropertyChanged;
        }

        private void Session_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_session.DisplayName))
            {
                RaisePropertyChanged(nameof(DisplayName));
            }
        }

        private void Children_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    Debug.Assert(e.NewItems.Count == 1);
                    ChildApps.Add(new AppItemViewModel((IAudioDeviceSession)e.NewItems[0], isChild:true));
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    Debug.Assert(e.OldItems.Count == 1);
                    ChildApps.Remove(ChildApps.First(x => x.Id == ((IAudioDeviceSession)e.OldItems[0]).Id));
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public void MoveAllSessionsToDevice(string id)
        {
            _session.MoveAllSessionsToDevice(id);
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

            _session.UpdatePeakValueBackground();
        }


        public void RefreshDisplayName()
        {
            _session.RefreshDisplayName();
        }

        public override float PeakValue => _session.PeakValue;

        public bool DoesGroupWith(IAppItemViewModel app) => (AppId == app.AppId);

        public override string ToString() => string.Format(IsMuted ? Properties.Resources.AppOrDeviceMutedFormatAccessibleText : Properties.Resources.AppOrDeviceFormatAccessibleText, DisplayName, Volume);
    }
}
