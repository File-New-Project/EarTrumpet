using EarTrumpet.DataModel.Audio;
using EarTrumpet.DataModel.WindowsAudio;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using static EarTrumpet.DataModel.Audio.ForegroundAppResolver;

namespace EarTrumpet.Tests;

internal static class ForegroundAppResolverTests
{
    public static IEnumerable<(string Name, Action Test)> Cases()
    {
        yield return ("Resolver uses nested leaf activity and IDs in either order", NestedLeaves);
        yield return ("Resolver never borrows active state from a sibling", InactiveDescendant);
        yield return ("Resolver retains all distinct active apps for ambiguity fallback", DistinctActiveApps);
        yield return ("Resolver handles empty and missing inputs", EmptyInputs);
        yield return ("Resolver requires intact strict ancestry and terminates cycles", StrictAncestry);
        yield return ("Resolver rejects reused direct and intermediate parent IDs", ReusedParentIds);
    }

    private static void NestedLeaves()
    {
        var processes = Snapshot((1, 0, 10), (2, 1, 20), (3, 0, 30));
        var active = new Session(2, "leaf-active", SessionState.Active);
        var inactive = new Session(3, "leaf-inactive", SessionState.Inactive);
        foreach (var leaves in new[] { new[] { inactive, active }, new[] { active, inactive } })
        {
            var inner = new Session(leaves[0].ProcessId, "grouping-parameter", SessionState.Active, leaves);
            var root = new Session(inner.ProcessId, "root-app", SessionState.Active, inner);
            var candidates = EnumerateSessionCandidates(new[] { root }).ToArray();

            Check.SequenceEqual(leaves.Select(leaf => (leaf.ProcessId, "root-app", leaf.State)),
                candidates.Select(candidate => (candidate.ProcessId, candidate.AppId, candidate.State)));
            Check.SequenceEqual(new[] { "root-app" }, GetActiveDescendantAppIds(1, processes, candidates));
        }
    }

    private static void InactiveDescendant()
    {
        var processes = Snapshot((1, 0, 10), (2, 1, 20), (3, 0, 30));
        var inner = new Session(2, "grouping-parameter", SessionState.Active,
            new Session(2, "inactive-descendant", SessionState.Inactive),
            new Session(3, "active-unrelated", SessionState.Active));
        var root = new Session(2, "root-app", SessionState.Active, inner);

        Check.Equal(0, GetActiveDescendantAppIds(1, processes,
            EnumerateSessionCandidates(new[] { root })).Count);
    }

    private static void DistinctActiveApps()
    {
        var processes = Snapshot((1, 0, 10), (2, 1, 20), (3, 2, 30));
        var candidates = new[]
        {
            new SessionCandidate(2, "z-app", SessionState.Active),
            new SessionCandidate(3, "a-app", SessionState.Active),
            new SessionCandidate(3, "z-app", SessionState.Active),
            new SessionCandidate(2, "expired", SessionState.Expired),
            new SessionCandidate(2, "moved", SessionState.Moved),
            new SessionCandidate(2, "inactive", SessionState.Inactive),
            new SessionCandidate(2, " ", SessionState.Active),
            new SessionCandidate(2, null, SessionState.Active),
            new SessionCandidate(1, "foreground", SessionState.Active),
        };

        Check.SequenceEqual(new[] { "a-app", "z-app" }, GetActiveDescendantAppIds(1, processes, candidates));
        Check.SequenceEqual(new[] { "a-app", "z-app" }, GetActiveDescendantAppIds(1, processes, candidates.Reverse()));
    }

    private static void EmptyInputs()
    {
        var processes = Snapshot((1, 0, 10), (2, 1, 20));
        var candidates = new[] { new SessionCandidate(2, "app", SessionState.Active) };
        Check.Equal(0, EnumerateSessionCandidates(null).Count());
        Check.Equal(0, EnumerateSessionCandidates(Array.Empty<IAudioDeviceSession>()).Count());
        Check.Equal(0, EnumerateSessionCandidates(new IAudioDeviceSession[] { null }).Count());
        Check.Equal(0, GetActiveDescendantAppIds(0, processes, candidates).Count);
        Check.Equal(0, GetActiveDescendantAppIds(1, null, candidates).Count);
        Check.Equal(0, GetActiveDescendantAppIds(1, processes, null).Count);
        Check.Equal(0, GetActiveDescendantAppIds(1, processes, Array.Empty<SessionCandidate>()).Count);
        Check.Equal(0, GetActiveDescendantAppIds(1, Snapshot(), candidates).Count);

        var leaf = new Session(2, "app", SessionState.Active);
        Check.SequenceEqual(new[] { "app" }, GetActiveDescendantAppIds(1, processes,
            EnumerateSessionCandidates(new[] { leaf })));
    }

    private static void StrictAncestry()
    {
        var processes = Snapshot((1, 0, 10), (2, 1, 20), (3, 2, 30),
            (4, 99, 40), (5, 6, 50), (6, 5, 50), (7, 7, 70), (8, 0, 80));
        foreach (var processId in new uint[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 99 })
        {
            var candidates = new[] { new SessionCandidate(processId, "app", SessionState.Active) };
            Check.Equal(processId == 2 || processId == 3 ? 1 : 0,
                GetActiveDescendantAppIds(1, processes, candidates).Count);
        }

        // Even a direct parent ID must resolve to a live entry in this snapshot.
        Check.Equal(0, GetActiveDescendantAppIds(1, Snapshot((2, 1, 20)),
            new[] { new SessionCandidate(2, "app", SessionState.Active) }).Count);
    }

    private static void ReusedParentIds()
    {
        var direct = new[] { new SessionCandidate(2, "app", SessionState.Active) };
        var descendant = new[] { new SessionCandidate(3, "app", SessionState.Active) };
        Check.Equal(0, GetActiveDescendantAppIds(1, Snapshot((1, 0, 30), (2, 1, 20)), direct).Count);
        Check.Equal(0, GetActiveDescendantAppIds(1,
            Snapshot((1, 0, 10), (2, 1, 40), (3, 2, 30)), descendant).Count);
        Check.Equal(0, GetActiveDescendantAppIds(1,
            Snapshot((1, 0, 40), (2, 1, 20), (3, 2, 30)), descendant).Count);
        Check.SequenceEqual(new[] { "app" }, GetActiveDescendantAppIds(1,
            Snapshot((1, 0, 10), (2, 1, 10), (3, 2, 20)), descendant));
    }

    private static IReadOnlyDictionary<uint, ProcessSnapshotEntry> Snapshot(params (uint Id, uint ParentId, long Time)[] entries)
        => entries.ToDictionary(entry => entry.Id, entry => new ProcessSnapshotEntry(entry.ParentId, entry.Time));

    private sealed class Session : IAudioDeviceSession
    {
        public Session(uint processId, string appId, SessionState state, params Session[] children)
        {
            ProcessId = processId;
            AppId = appId;
            State = state;
            Children = new ObservableCollection<IAudioDeviceSession>(children);
        }

        public uint ProcessId { get; }
        public string AppId { get; }
        public SessionState State { get; }
        public ObservableCollection<IAudioDeviceSession> Children { get; }
        public event PropertyChangedEventHandler PropertyChanged { add { } remove { } }
        public IEnumerable<IAudioDeviceSessionChannel> Channels => throw new NotSupportedException();
        public IAudioDevice Parent => throw new NotSupportedException();
        public string DisplayName => throw new NotSupportedException();
        public string ExeName => throw new NotSupportedException();
        public string IconPath => throw new NotSupportedException();
        public bool IsDesktopApp => throw new NotSupportedException();
        public bool IsSystemSoundsSession => throw new NotSupportedException();
        public string PackageInstallPath => throw new NotSupportedException();
        public string Id => throw new NotSupportedException();
        public bool IsMuted { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public float Volume { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public float PeakValue1 => throw new NotSupportedException();
        public float PeakValue2 => throw new NotSupportedException();
        public float GetVolumeScalar() => throw new NotSupportedException();
        public float GetVolumeLogarithmic() => throw new NotSupportedException();
        public void SetVolumeScalar(float value) => throw new NotSupportedException();
        public void SetVolumeLogarithmic(float value) => throw new NotSupportedException();
    }
}
