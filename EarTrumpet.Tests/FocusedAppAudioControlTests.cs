using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;

namespace EarTrumpet.Tests;

internal static class FocusedAppAudioControlTests
{
    internal static IEnumerable<(string Name, Action Test)> Cases()
    {
        yield return ("Volume steps preserve levels across nested groups and devices", PreservesLevels);
        yield return ("Logarithmic steps preserve each stream's level", PreservesLogarithmicLevels);
        yield return ("Volume steps clamp each stream independently", ClampsVolume);
        yield return ("Non-finite volumes recover at the current mode's minimum", RecoversNonFiniteVolume);
        yield return ("Mixed mute states mute every stream regardless of child order", MutesMixedStreams);
        yield return ("All-muted streams unmute across devices", UnmutesAllStreams);
        yield return ("Leaf placeholders retain volume and mute changes", UpdatesPlaceholders);
        yield return ("Empty targets are harmless", HandlesEmptyTargets);
        yield return ("Volume changes use a snapshot of target streams", SnapshotsTargets);
    }

    private static void PreservesLevels()
    {
        var first = new TestApp(50);
        var second = new TestApp(10);
        var otherDevice = new TestApp(70);
        var targets = new[] { Group(Group(first, second)), Group(Group(otherDevice)) };

        FocusedAppAudioControl.ChangeVolume(targets, -2, 0, 100);
        Check.SequenceEqual(new[] { 48f, 8f, 68f }, new[] { first.Volume, second.Volume, otherDevice.Volume });

        FocusedAppAudioControl.ChangeVolume(targets, 2, 0, 100);
        Check.SequenceEqual(new[] { 50f, 10f, 70f }, new[] { first.Volume, second.Volume, otherDevice.Volume });
    }

    private static void PreservesLogarithmicLevels()
    {
        var first = new TestApp(-5);
        var second = new TestApp(-20);
        var targets = new[] { Group(Group(first, second)) };
        FocusedAppAudioControl.ChangeVolume(targets, -0.5f, -40, 0);
        FocusedAppAudioControl.ChangeVolume(targets, -0.5f, -40, 0);
        Check.Equal(-6f, first.Volume);
        Check.Equal(-21f, second.Volume);
    }

    private static void ClampsVolume()
    {
        foreach (var (volume, delta, minimum, maximum, expected) in new[]
        {
            (1f, -2f, 0f, 100f, 0f),
            (99f, 2f, 0f, 100f, 100f),
            (0f, -2f, 0f, 100f, 0f),
            (100f, 2f, 0f, 100f, 100f),
            (-39.75f, -0.5f, -40f, 0f, -40f),
            (-0.25f, 0.5f, -40f, 0f, 0f),
            (-60f, -0.5f, -60f, 0f, -60f),
        })
        {
            var app = new TestApp(volume);
            FocusedAppAudioControl.ChangeVolume(new[] { Group(Group(app)) }, delta, minimum, maximum);
            Check.Equal(expected, app.Volume);
        }
    }

    private static void RecoversNonFiniteVolume()
    {
        foreach (var invalid in new[] { float.NaN, float.NegativeInfinity, float.PositiveInfinity })
        {
            foreach (var (minimum, maximum, delta) in new[] { (0f, 100f, 2f), (-40f, 0f, 0.5f) })
            {
                var app = new TestApp(invalid);
                FocusedAppAudioControl.ChangeVolume(new[] { app }, delta, minimum, maximum);
                Check.Equal(minimum + delta, app.Volume);
            }
        }
    }

    private static void MutesMixedStreams()
    {
        foreach (var firstMuted in new[] { true, false })
        {
            var first = new TestApp(50) { IsMuted = firstMuted };
            var second = new TestApp(10) { IsMuted = !firstMuted };
            var otherDevice = new TestApp(70) { IsMuted = true };
            FocusedAppAudioControl.ToggleMute(new[] { Group(Group(first, second)), Group(otherDevice) });
            Check.True(first.IsMuted && second.IsMuted && otherDevice.IsMuted);
        }
    }

    private static void UnmutesAllStreams()
    {
        var first = new TestApp(50) { IsMuted = true };
        var second = new TestApp(10) { IsMuted = true };
        FocusedAppAudioControl.ToggleMute(new[] { Group(Group(first)), Group(Group(second)) });
        Check.True(!first.IsMuted && !second.IsMuted);
    }

    private static void UpdatesPlaceholders()
    {
        var first = new TestApp(50) { ChildApps = new ObservableCollection<IAppItemViewModel>() };
        var second = new TestApp(10);
        var placeholder = Group(Group(first, second));
        FocusedAppAudioControl.ChangeVolume(new[] { placeholder }, -2, 0, 100);
        FocusedAppAudioControl.ToggleMute(new[] { placeholder });
        Check.Equal(48f, first.Volume);
        Check.Equal(8f, second.Volume);
        Check.True(first.IsMuted && second.IsMuted);
    }

    private static void HandlesEmptyTargets()
    {
        FocusedAppAudioControl.ChangeVolume(Array.Empty<IAppItemViewModel>(), 2, 0, 100);
        FocusedAppAudioControl.ToggleMute(Array.Empty<IAppItemViewModel>());
    }

    private static void SnapshotsTargets()
    {
        var first = new TestApp(50);
        var second = new TestApp(10);
        var group = Group(first, second);
        first.VolumeChanged = () => group.ChildApps.Remove(second);
        FocusedAppAudioControl.ChangeVolume(new[] { group }, -2, 0, 100);
        Check.Equal(48f, first.Volume);
        Check.Equal(8f, second.Volume);
    }

    private static TestApp Group(params IAppItemViewModel[] children) => new(0)
    {
        ChildApps = new ObservableCollection<IAppItemViewModel>(children),
    };

    // Match mixer group semantics: reads use the first child, writes affect all children.
    private sealed class TestApp(float volume) : BindableBase, IAppItemViewModel
    {
        private float _volume = volume;
        private bool _muted;
        public Action VolumeChanged { get; set; }
        public ObservableCollection<IAppItemViewModel> ChildApps { get; set; }
        public float Volume
        {
            get => ChildApps?.Count > 0 ? ChildApps[0].Volume : _volume;
            set
            {
                if (ChildApps?.Count > 0)
                {
                    foreach (var child in ChildApps) child.Volume = value;
                }
                else _volume = value;
                VolumeChanged?.Invoke();
            }
        }
        public bool IsMuted
        {
            get => ChildApps?.Count > 0 ? ChildApps[0].IsMuted : _muted;
            set
            {
                if (ChildApps?.Count > 0)
                {
                    foreach (var child in ChildApps) child.IsMuted = value;
                }
                else _muted = value;
            }
        }
        public string Id => "session";
        public string AppId => "app";
        public string DisplayName => "Test app";
        public string ExeName => AppId;
        public string PackageInstallPath => null;
        public string IconPath => null;
        public Color Background => default;
        public char IconText => 'T';
        public bool IsDesktopApp => true;
        public bool IsExpanded => false;
        public bool IsMovable => false;
        public float PeakValue1 => 0;
        public float PeakValue2 => 0;
        public string PersistedOutputDevice => null;
        public uint ProcessId => 0;
        public IDeviceViewModel Parent => null;
        public bool DoesGroupWith(IAppItemViewModel app) => AppId == app.AppId;
        public void MoveToDevice(string id, bool hide) => throw new NotSupportedException();
        public void UpdatePeakValueForeground() { }
        public void UpdatePeakValueBackground() { }
    }
}
