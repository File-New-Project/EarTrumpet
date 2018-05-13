﻿using EarTrumpet.DataModel;
using EarTrumpet.Extensions;
using EarTrumpet.Services;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EarTrumpet.ViewModels
{
    public class AppItemViewModel : AudioSessionViewModel
    {
        private IAudioDeviceSession _session;

        public string ExeName { get; private set; }

        public ImageSource Icon { get; private set; }

        public SolidColorBrush Background { get; private set; }

        public char IconText => DisplayName.ToUpperInvariant().FirstOrDefault(x => char.IsLetterOrDigit(x));

        string _displayName;
        public override string DisplayName => _displayName;

        bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                _isExpanded = value;
                RaisePropertyChanged(nameof(IsExpanded));
            }
        }

        public ObservableCollection<AppItemViewModel> ChildApps { get; private set; }

        public bool IsMovable => !_session.IsSystemSoundsSession;

        public string MoveIcon => IsMovable ? "\uE8AB" : "";

        public SimpleAudioDeviceViewModel PersistedOutputDevice
        {
            get
            {
                string deviceId = AudioPolicyConfigService.GetDefaultEndPoint(_session.ProcessId);
                if (string.IsNullOrWhiteSpace(deviceId))
                {
                    return MainViewModel.Instance.DefaultPlaybackDevice;
                }
                else
                {
                    return MainViewModel.Instance.PlaybackDevices.First(d => d.Id == deviceId);
                }
            }
            set
            {
                if (value == null) return;

                AudioPolicyConfigService.SetDefaultEndPoint(value.Id, _session.ProcessId);
            }
        }

        public AppItemViewModel(IAudioDeviceSession session) : base(session)
        {
            _session = session;

            ChildApps = new ObservableCollection<AppItemViewModel>();
            if (_session.Children != null)
            {
                _session.Children.CollectionChanged += Children_CollectionChanged;
                LoadChildren();
            }

            ExeName = session.DisplayName;
            _displayName = ExeName;

            if (session.DisplayName.ToLowerInvariant() == "speechruntime.exe")
            {
                _displayName = Properties.Resources.SpeechRuntimeDisplayName;
            }
            else if (session.IsSystemSoundsSession)
            {
                _displayName = Properties.Resources.SystemSoundsDisplayName;
            }

            if (session.IsDesktopApp)
            {                
                try
                {
                    if (Path.GetExtension(session.IconPath) == ".dll")
                    {
                        Icon = IconService.GetIconFromFileAsImageSource(session.IconPath);
                    }
                    else
                    {
                        // override for SpeechRuntime.exe (Repo -> HEY CORTANA)
                        if (session.IconPath.ToLowerInvariant().Contains("speechruntime.exe"))
                        {
                            var sysType = Environment.Is64BitOperatingSystem ? "SysNative" : "System32";
                            Icon = IconService.GetIconFromFileAsImageSource(Path.Combine("%windir%", sysType, "Speech\\SpeechUX\\SpeechUXWiz.exe"), 0);
                        }
                        else
                        {
                            Icon = System.Drawing.Icon.ExtractAssociatedIcon(session.IconPath).ToImageSource();
                        }
                    }
                }
                catch(Exception ex)
                {
                    Debug.WriteLine($"Failed to load icon: {ex}");
                    // ignored
                }

                Background = new SolidColorBrush(Icon == null ?
                    AccentColorService.GetColorByTypeName("ImmersiveSystemAccent") :
                    Colors.Transparent);
            }
            else
            {
                if (File.Exists(session.IconPath)) // hack until we invoke the resource manager correctly.
                {
                    Icon = new BitmapImage(new Uri(session.IconPath));
                }
                Background = new SolidColorBrush(AccentColorService.FromABGR(session.BackgroundColor));
            }

            Task.Factory.StartNew(() =>
            {
                try
                {
                    var proc = Process.GetProcessById((int)_session.ProcessId);
                    if (!string.IsNullOrWhiteSpace(proc.MainWindowTitle))
                    {
                        var displayName = proc.MainWindowTitle;
                        App.Current.Dispatcher.SafeInvoke(() =>
                        {
                            _displayName = displayName;
                            RaisePropertyChanged(nameof(DisplayName));
                        });
                    }
                }
                catch { } // we fallback to exe name if DisplayName is not set in the try above.
            });
        }

        private void LoadChildren()
        {
            foreach(var child in _session.Children)
            {
                ChildApps.Add(new AppItemViewModel(child));
            }
        }

        private void Children_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    Debug.Assert(e.NewItems.Count == 1);
                    ChildApps.Add(new AppItemViewModel((IAudioDeviceSession)e.NewItems[0]));
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    Debug.Assert(e.OldItems.Count == 1);
                    ChildApps.Remove(ChildApps.First(x => x.Id == ((IAudioDeviceSession)e.OldItems[0]).Id));
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    throw new NotImplementedException();

                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException();

                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    throw new NotImplementedException();
            }
        }

        public override void TriggerPeakCheck()
        {
            if (ChildApps != null)
            {
                foreach(var child in ChildApps)
                {
                    child.TriggerPeakCheck();
                }
            }

            base.TriggerPeakCheck();
        }

        public override float PeakValue
        {
            get
            {
                if (_session.Children != null)
                {
                    float highestOfChildrenPeakValues = 0;
                    foreach(var child in _session.Children)
                    {
                        var childPeakValue = child.PeakValue;
                        if (childPeakValue > highestOfChildrenPeakValues)
                        {
                            highestOfChildrenPeakValues = childPeakValue;
                        }
                    }
                    return highestOfChildrenPeakValues;
                }
                else
                {
                    return _session.PeakValue;
                }
            }
        }
    }
}
