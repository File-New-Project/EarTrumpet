using EarTrumpet.DataModel;
using EarTrumpet.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EarTrumpet.ViewModels
{
    public class AppItemViewModel : AudioSessionViewModel
    {
        public class ExeNameComparer : IComparer<AppItemViewModel>
        {
            public int Compare(AppItemViewModel one, AppItemViewModel two)
            {
                return string.Compare(one._session.ExeName, two._session.ExeName, StringComparison.Ordinal);
            }
        }

        public static readonly ExeNameComparer CompareByExeName = new ExeNameComparer();

        public ImageSource Icon { get; private set; }

        public SolidColorBrush Background { get; private set; }

        public char IconText => DisplayName.ToUpperInvariant().FirstOrDefault(x => char.IsLetterOrDigit(x));

        public string DisplayName => _session.DisplayName;

        public ObservableCollection<AppItemViewModel> ChildApps { get; private set; }

        public bool IsMovable => !_session.IsSystemSoundsSession;

        public string PersistedOutputDevice => AudioPolicyConfigService.GetDefaultEndPoint(_session.ProcessId);

        public bool IsExpanded { get; private set; }

        private IAudioDeviceSession _session;

        public AppItemViewModel(IAudioDeviceSession session, bool isChild = false) : base(session)
        {
            IsExpanded = isChild;
            _session = session;
            _session.PropertyChanged += Session_PropertyChanged;

            if (_session.Children != null)
            {
                _session.Children.CollectionChanged += Children_CollectionChanged;
                ChildApps = new ObservableCollection<AppItemViewModel>(_session.Children.Select(t => new AppItemViewModel(t, isChild: true)));
            }

            Background = new SolidColorBrush(session.IsDesktopApp ? Colors.Transparent :
                AccentColorService.FromABGR(session.BackgroundColor));

            try
            {
                if (session.IsDesktopApp)
                {
                    Icon = IconService.GetIconFromFileAsImageSource(session.IconPath);
                }
                else
                {
                    Icon = new BitmapImage(new Uri(session.IconPath));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load icon: {ex}");
            }
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

        public override void TriggerPeakCheck()
        {
            if (ChildApps != null)
            {
                foreach (var child in ChildApps)
                {
                    child.TriggerPeakCheck();
                }
            }

            base.TriggerPeakCheck();
        }

        public override float PeakValue => _session.PeakValue;

        public bool DoesGroupWith(AppItemViewModel app) => (_session.AppId == app._session.AppId);

        public override string ToString() => string.Format(IsMuted ? Properties.Resources.AppMutedFormatAccessibleText : Properties.Resources.AppFormatAccessibleText, DisplayName, Volume);
    }
}
