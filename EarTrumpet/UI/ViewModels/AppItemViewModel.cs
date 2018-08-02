using EarTrumpet.DataModel;
using EarTrumpet.Extensions;
using EarTrumpet.Interop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.ComponentModel;

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

        public ImageSource Icon { get; private set; }

        public SolidColorBrush Background { get; private set; }

        public char IconText => string.IsNullOrWhiteSpace(DisplayName) ? '?' : DisplayName.ToUpperInvariant().FirstOrDefault(x => char.IsLetterOrDigit(x));

        public string DisplayName => _session.SessionDisplayName;

        public string ExeName => _session.ExeName;
        public string AppId => _session.AppId;

        public ObservableCollection<IAppItemViewModel> ChildApps { get; private set; }

        public bool IsMovable => !_session.IsSystemSoundsSession && 
                                  Environment.OSVersion.IsAtLeast(OSVersions.RS4);

        public string PersistedOutputDevice => _session.PersistedDefaultEndPointId;

        public bool IsExpanded { get; private set; }

        public int ProcessId => _session.ProcessId;

        private IAudioDeviceSession _session;
        private WeakReference<DeviceViewModel> _parent;

        internal AppItemViewModel(DeviceViewModel parent, IAudioDeviceSession session, bool isChild = false, ImageSource icon = null) : base(session)
        {
            IsExpanded = isChild;
            _session = session;
            _session.PropertyChanged += Session_PropertyChanged;
            _parent = new WeakReference<DeviceViewModel>(parent);

            Background = new SolidColorBrush(session.IsDesktopApp ? Colors.Transparent : session.BackgroundColor.ToABGRColor());

            if (isChild)
            {
                Icon = icon;
            }
            else
            {
                try
                {
                    if (session.IsDesktopApp)
                    {
                        if (!string.IsNullOrWhiteSpace(session.IconPath))
                        {
                            var iconPath = new StringBuilder(session.IconPath);
                            int iconIndex = Shlwapi.PathParseIconLocationW(iconPath);
                            if (iconIndex != 0)
                            {
                                Icon = GetIconFromFile(iconPath.ToString(), iconIndex);
                            }
                            else
                            {
                                Icon = System.Drawing.Icon.ExtractAssociatedIcon(session.IconPath).ToImageSource();
                            }
                        }
                    }
                    else
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.UriSource = new Uri(session.IconPath);
                        bitmap.EndInit();

                        Icon = bitmap;
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceError($"Failed to load icon: {ex}");
                }
            }

            if (_session.Children != null)
            {
                _session.Children.CollectionChanged += Children_CollectionChanged;
                ChildApps = new ObservableCollection<IAppItemViewModel>(_session.Children.Select(t => new AppItemViewModel(parent, t, true, Icon)));
            }
        }

        ~AppItemViewModel()
        {
            _session.PropertyChanged -= Session_PropertyChanged;
        }

        private ImageSource GetIconFromFile(string path, int iconIndex = 0)
        {
            IntPtr iconHandle = IntPtr.Zero;
            try
            {
                iconHandle = Shell32.ExtractIcon(Process.GetCurrentProcess().Handle, path, iconIndex);
                return Imaging.CreateBitmapSourceFromHIcon(iconHandle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                User32.DestroyIcon(iconHandle);
            }
        }

        private void Session_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_session.SessionDisplayName))
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
                        ChildApps.Add(new AppItemViewModel(parent, (IAudioDeviceSession)e.NewItems[0], true, Icon));
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
            _session.MoveToDevice(id, hide);
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

        public bool DoesGroupWith(IAppItemViewModel app) => (AppId == app.AppId);

        public override string ToString() => string.Format(IsMuted ? Properties.Resources.AppOrDeviceMutedFormatAccessibleText : Properties.Resources.AppOrDeviceFormatAccessibleText, DisplayName, Volume);

        public void OpenPopup(UIElement container)
        {
            if (_parent.TryGetTarget(out var parent))
            {
                parent.OpenPopup(this, container);
            }
        }
    }
}
