using EarTrumpet.DataModel.Storage;
using EarTrumpet.DataModel.WindowsAudio.Internal;
using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Windows.Win32.Foundation;
using Windows.Win32.Media.Audio;

namespace EarTrumpet.Tests;

internal static class AudioDeviceSessionVolumeTests
{
    internal static IEnumerable<(string Name, Action Test)> Cases()
    {
        yield return ("Repeated logarithmic shortcuts work before audio callbacks arrive", RepeatedLogarithmicSteps);
        yield return ("Logarithmic writes preserve scalar readback at boundaries", ScalarReadback);
    }

    private static void RepeatedLogarithmicSteps()
    {
        using var fixture = new SessionFixture();
        fixture.Session.SetVolumeScalar(0.1f);
        var app = new AppItemViewModel(null, fixture.Session);

        for (var step = 1; step <= 3; step++)
        {
            FocusedAppAudioControl.ChangeVolume(new[] { app }, -0.5f, -40, 0);
            Near(-20f - step * 0.5f, app.Volume);
            Near(fixture.Endpoint.Scalar, fixture.Session.GetVolumeScalar());
        }
    }

    private static void ScalarReadback()
    {
        using var fixture = new SessionFixture();
        foreach (var (requested, expectedDb, expectedScalar) in new[]
        {
            (-20f, -20f, 0.1f), (-50f, -40f, 0.01f), (10f, 0f, 1f),
        })
        {
            fixture.Session.SetVolumeLogarithmic(requested);
            Near(expectedDb, fixture.Session.GetVolumeLogarithmic());
            Near(expectedScalar, fixture.Session.GetVolumeScalar());
            Near(expectedScalar, fixture.Endpoint.Scalar);
        }
    }

    private static void Near(float expected, float actual)
    {
        if (!float.IsFinite(actual) || Math.Abs(expected - actual) > 0.0001f)
        {
            throw new InvalidOperationException($"Expected approximately {expected}, got {actual}.");
        }
    }

    private sealed class SessionFixture : IDisposable
    {
        private readonly AppSettings _previousSettings = App.Settings;
        public AudioDeviceSession Session { get; }
        public SimpleVolume Endpoint { get; } = new();

        public SessionFixture()
        {
            // Exercise the real view model and session setters without a COM connection,
            // audio callbacks, or access to the user's saved settings.
            var settings = new AppSettings();
            typeof(AppSettings).GetField("_settings", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(settings, new LogarithmicSettings());
            typeof(App).GetProperty(nameof(App.Settings)).SetValue(null, settings);
            Session = (AudioDeviceSession)RuntimeHelpers.GetUninitializedObject(typeof(AudioDeviceSession));
            GC.SuppressFinalize(Session);
            typeof(AudioDeviceSession).GetField("_simpleVolume", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(Session, Endpoint);
        }

        public void Dispose() => typeof(App).GetProperty(nameof(App.Settings)).SetValue(null, _previousSettings);
    }

    private sealed class LogarithmicSettings : ISettingsBag
    {
        public string Namespace => "";
        public bool HasKey(string key) => key == nameof(AppSettings.UseLogarithmicVolume);
        public T Get<T>(string key, T defaultValue) => key == nameof(AppSettings.UseLogarithmicVolume)
            ? (T)(object)true : defaultValue;
        public void Set<T>(string key, T value) => throw new NotSupportedException();
        public event EventHandler<string> SettingChanged { add { } remove { } }
    }

    private sealed unsafe class SimpleVolume : ISimpleAudioVolume
    {
        public float Scalar { get; private set; }
        private BOOL _muted;
        public void SetMasterVolume(float value, Guid* context) => Scalar = value;
        public void GetMasterVolume(out float value) => value = Scalar;
        public void SetMute(BOOL value, Guid* context) => _muted = value;
        public void GetMute(BOOL* value) => *value = _muted;
    }
}
