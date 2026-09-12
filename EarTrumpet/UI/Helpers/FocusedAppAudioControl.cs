using EarTrumpet.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EarTrumpet.UI.Helpers;

internal static class FocusedAppAudioControl
{
    internal static void ChangeVolume(IEnumerable<IAppItemViewModel> apps, float delta, float minimum, float maximum)
    {
        foreach (var stream in EnumerateStreams(apps).ToArray())
        {
            var volume = stream.Volume;
            var currentVolume = float.IsFinite(volume) ? volume : minimum;
            stream.Volume = Math.Clamp(currentVolume + delta, minimum, maximum);
        }
    }

    internal static void ToggleMute(IEnumerable<IAppItemViewModel> apps)
    {
        var streams = EnumerateStreams(apps).ToArray();
        var shouldMute = streams.Any(stream => !stream.IsMuted);
        foreach (var stream in streams)
        {
            stream.IsMuted = shouldMute;
        }
    }

    private static IEnumerable<IAppItemViewModel> EnumerateStreams(IEnumerable<IAppItemViewModel> apps)
    {
        foreach (var app in apps)
        {
            var children = app.ChildApps?.ToArray();
            if (children?.Length > 0)
            {
                // Group getters describe only the first stream, while setters affect every stream.
                // Walk all grouping levels to preserve individual volumes and inspect every mute state.
                foreach (var stream in EnumerateStreams(children))
                {
                    yield return stream;
                }
            }
            else
            {
                // Include leaf placeholders so changes survive an inactive app's device move.
                yield return app;
            }
        }
    }
}
