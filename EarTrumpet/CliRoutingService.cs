using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using EarTrumpet.DataModel.Audio;
using EarTrumpet.DataModel.WindowsAudio;
using System.Windows.Threading;
using EarTrumpet.Interop.Helpers;


namespace EarTrumpet.CLIServices
{
    public class CliRoutingService
    {

        private readonly App _app;

        private readonly List<IAudioDeviceManager> _managers = new List<IAudioDeviceManager>();
        private IAudioDeviceManager _singleManager; 
        private string _targetExeName;
        private string _targetDeviceName;

        public CliRoutingService(App app)
        {
            _app = app;
        }

        public bool TryHandleCliArgs(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                return false;
            }

            var a0 = NormalizeArg(args[0]);

            // --help / -h / /? / help
            if (IsHelpArg(a0))
            {
                PrintUsageAndExit(0);
                return true;
            }

            // --list-devices
            if (string.Equals(a0, "list-devices", StringComparison.OrdinalIgnoreCase))
            {
                StartCliListDevices();
                return true;
            }

            // --list-apps
            if (string.Equals(a0, "list-apps", StringComparison.OrdinalIgnoreCase))
            {
                StartCliListApps();
                return true;
            }

            // --set "app.exe" "Device Display Name"
            if (string.Equals(a0, "set", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(args[0], "--set", StringComparison.OrdinalIgnoreCase))
            {
                if (args.Length < 3)
                {
                    TryAttachConsole();
                    Console.Error.WriteLine("Usage: --set \"[app.exe]\" \"[Playback audio device name]\"");
                    HardExit(1);
                    return true;
                }

                _targetExeName = args[1];
                _targetDeviceName = args[2];

                StartCliSetRoute();
                return true;
            }

            PrintUsageAndExit(0);
            HardExit(0);
            return false;
        }

        private void StartCliSetRoute()
        {
            try
            {
                _singleManager = WindowsAudioFactory.CreateNonSharedDeviceManager(AudioDeviceKind.Playback);
                _singleManager.Loaded += OnSetRouteManagerLoaded;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"CLI: Failed to start - {ex}");
                HardExit(10);
            }
        }

        private void OnSetRouteManagerLoaded(object sender, EventArgs e)
        {
            try { _singleManager.Loaded -= OnSetRouteManagerLoaded; } catch { }

            int exitCode = 0;
            try
            {
                exitCode = RouteProcessesToDevice();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"CLI: Unexpected failure: {ex}");
                exitCode = 20;
            }

            HardExit(exitCode);
        }
    
        private void StartCliListDevices()
        {
            try
            {
                foreach (AudioDeviceKind kind in Enum.GetValues(typeof(AudioDeviceKind)))
                {
                    try
                    {
                        var mgr = WindowsAudioFactory.CreateNonSharedDeviceManager(kind);
                        mgr.Loaded += OnListDevicesManagerLoaded;
                        _managers.Add(mgr);
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"CLI: Failed to create manager for kind {kind}: {ex}");
                    }
                }

                if (_managers.Count == 0)
                {
                    TryAttachConsole();
                    Console.Error.WriteLine("Failed to create audio device managers.");
                    HardExit(2);
                }
            }
            catch (Exception ex)
            {
                TryAttachConsole();
                Console.Error.WriteLine("Failed to start device listing.");
                Console.Error.WriteLine(ex.ToString());
                HardExit(2);
            }
        }
        

        private int _loadedManagersForListDevices = 0;
        private void OnListDevicesManagerLoaded(object sender, EventArgs e)
        {
            _loadedManagersForListDevices++;
            if (_loadedManagersForListDevices >= _managers.Count)
            {
                int exitCode = 0;
                try
                {
                    var lines = new List<string>();
                    foreach (var mgr in _managers)
                    {
                        var def = mgr.Default;
                        foreach (var d in mgr.Devices.OrderBy(x => x.DisplayName))
                        {
                            var isDefault = ReferenceEquals(d, def) ? " (Default)" : "";
                            lines.Add($"[{mgr.Kind}] {d.DisplayName}{isDefault}");
                        }
                    }

                    TryAttachConsole();
                    foreach (var line in lines) Console.WriteLine(line);
                    HardExit(exitCode);
                }
                catch (Exception ex)
                {
                    TryAttachConsole();
                    Console.Error.WriteLine("Failed to list devices.");
                    Console.Error.WriteLine(ex.ToString());
                    HardExit(2);
                }
            }
        }
        private void StartCliListApps()
        {
            try
            {
                _singleManager = WindowsAudioFactory.CreateNonSharedDeviceManager(AudioDeviceKind.Playback);
                _singleManager.Loaded += OnListAppsManagerLoaded;
            }
            catch (Exception ex)
            {
                TryAttachConsole();
                Console.Error.WriteLine("Failed to start app listing.");
                Console.Error.WriteLine(ex.ToString());
                HardExit(2);
            }
        }

        private void OnListAppsManagerLoaded(object sender, EventArgs e)
        {

            try { _singleManager.Loaded -= OnListAppsManagerLoaded; } catch { }

            try
            {
                var exes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var dev in _singleManager.Devices)
                {
                    foreach (var sess in dev.Groups)
                    {
                        var exe = GetExeNameFromSession(sess);
                        if (!string.IsNullOrWhiteSpace(exe))
                        {
                            exes.Add(exe);
                        }
                    }
                }

                var list = exes.OrderBy(x => x).ToList();

                TryAttachConsole();
                foreach (var x in list) Console.WriteLine(x);
                HardExit(0);
            }
            catch (Exception ex)
            {
                TryAttachConsole();
                Console.Error.WriteLine("Failed to list apps.");
                Console.Error.WriteLine(ex.ToString());
                HardExit(2);
            }
        }

        // 0 = success (routed at least one PID)
        // 1 = invalid exe name
        // 2 = no process found
        // 3 = device not found
        // 4 = nothing routed (processes found but all failed)
        private int RouteProcessesToDevice()
        {
            var exeNoExt = Path.GetFileNameWithoutExtension(_targetExeName);
            if (string.IsNullOrWhiteSpace(exeNoExt))
            {
                return 1;
            }

            var processes = Process.GetProcessesByName(exeNoExt);
            if (processes == null || processes.Length == 0)
            {
                return 2;
            }

            var targetDevice = FindTargetDevice();
            if (targetDevice == null)
            {
                return 3;
            }

            var mgr = (IAudioDeviceManagerWindowsAudio)_singleManager;
            int success = 0;

            foreach (var p in processes)
            {
                try
                {
                    mgr.SetDefaultEndPoint(targetDevice.Id, p.Id);
                    success++;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"CLI: Failed to route PID {p?.Id}: {ex}");
                }
            }

            return success > 0 ? 0 : 4;
        }

        private IAudioDevice FindTargetDevice()
        {
            var devices = _singleManager.Devices;
            return devices.FirstOrDefault(d => string.Equals(d.DisplayName, _targetDeviceName, StringComparison.OrdinalIgnoreCase)) ??
                   devices.FirstOrDefault(d => (d.DisplayName ?? "").IndexOf(_targetDeviceName, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private void PrintUsageAndExit(int code)
        {
            TryAttachConsole();
            var exe = AppDomain.CurrentDomain.FriendlyName;
            Console.WriteLine("EarTrumpet CLI");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine($"  {exe} --help            Show this help");
            Console.WriteLine($"  {exe} --list-devices    List audio devices");
            Console.WriteLine($"  {exe} --list-apps       List audio applications (exe names)");
            Console.WriteLine($"  {exe} --set \"[app.exe]\" \"[Playback audio device name]\"  Route app to playback device");
            Console.WriteLine();
            Console.WriteLine("Notes:");
            Console.WriteLine("  '-', '--', and '/' prefixes are supported (e.g., -h, --help, /help).");
            HardExit(code);
        }

        private static bool IsHelpArg(string arg)
        {
            var a = NormalizeArg(arg);
            return a == "h" || a == "help" || a == "?";
        }

        private static string NormalizeArg(string arg)
        {
            if (string.IsNullOrWhiteSpace(arg)) return string.Empty;
            var trimmed = arg.Trim().TrimStart('-', '/');
            return trimmed.ToLowerInvariant();
        }

        private static string GetExeNameFromSession(object sess)
        {
            var exe = GetProp<string>(sess, "ExeName");
            if (!string.IsNullOrWhiteSpace(exe)) return Path.GetFileName(exe);

            var appId = GetProp<string>(sess, "AppId");
            if (!string.IsNullOrWhiteSpace(appId)) return Path.GetFileName(appId);

            var display = GetProp<string>(sess, "DisplayName");
            if (!string.IsNullOrWhiteSpace(display)) return Path.GetFileName(display);

            var isSystem = GetProp<bool?>(sess, "IsSystemSoundsSession") ?? false;
            if (isSystem) return "System Sounds";

            return null;
        }

        private static T GetProp<T>(object obj, string prop)
        {
            try
            {
                var pi = obj.GetType().GetProperty(prop);
                if (pi == null) return default;
                var val = pi.GetValue(obj);
                if (val == null) return default;
                if (typeof(T).IsAssignableFrom(val.GetType())) return (T)val;
                return (T)Convert.ChangeType(val, typeof(T));
            }
            catch
            {
                return default;
            }
        }

        private void HardExit(int code)
        {
            Environment.Exit(code);
        }

        private static bool _consoleAttached = false;

        private static void TryAttachConsole()
        {
            if (_consoleAttached) return;

            try
            {
                if (!AttachConsole(ATTACH_PARENT_PROCESS))
                {
                }
            }
            catch {  }
            finally
            {
                _consoleAttached = true;
            }
        }

        private const int ATTACH_PARENT_PROCESS = -1;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AttachConsole(int dwProcessId);
    }


    internal static class DeviceReader
    {

       
        public static IEnumerable<string> FromEarTrumpet()
        {
            var tcs = new TaskCompletionSource<List<string>>();

            var thread = new Thread(() =>
            {
                var dispatcher = Dispatcher.CurrentDispatcher;

                try
                {
                    var earAsm = typeof(IAudioDeviceManager).Assembly;

                    var mgrType = earAsm
                        .GetTypes()
                        .FirstOrDefault(t =>
                            t.IsClass &&
                            !t.IsAbstract &&
                            typeof(IAudioDeviceManager).IsAssignableFrom(t) &&
                            t.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                             .Any(c =>
                             {
                                 var ps = c.GetParameters();
                                 return ps.Length == 1 && ps[0].ParameterType.IsEnum;
                             }));

                    if (mgrType == null)
                        throw new TypeLoadException("Could not locate a concrete IAudioDeviceManager with a single enum constructor in the EarTrumpet assembly.");

                    var ctor = mgrType
                        .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        .First(c =>
                        {
                            var ps = c.GetParameters();
                            return ps.Length == 1 && ps[0].ParameterType.IsEnum;
                        });

                    var enumType = ctor.GetParameters()[0].ParameterType;

                    var enumValues = Enum.GetValues(enumType).Cast<object>().ToList();

                    var managers = enumValues
                        .Select(val => (IAudioDeviceManager)ctor.Invoke(new object[] { val }))
                        .ToList();

                    int loadedCount = 0;
                    var frame = new DispatcherFrame();

                    EventHandler onLoaded = (s, e) =>
                    {
                        loadedCount++;
                        if (loadedCount >= managers.Count)
                        {
                            var lines = new List<string>();
                            foreach (var mgr in managers)
                            {
                                lines.AddRange(RenderManagerDevices(mgr));
                            }
                            tcs.TrySetResult(lines);
                            frame.Continue = false;
                        }
                    };

                    foreach (var mgr in managers)
                    {
                        mgr.Loaded += onLoaded;
                    }

                    Dispatcher.PushFrame(frame);

                    dispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                    dispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();

            return tcs.Task.GetAwaiter().GetResult();
        }

        private static IEnumerable<string> RenderManagerDevices(IAudioDeviceManager mgr)
        {
            var kind = mgr.Kind; 
            var def = mgr.Default;

            return mgr.Devices
                .OrderBy(d => d.DisplayName)
                .Select(d =>
                {
                    var isDefault = ReferenceEquals(d, def) ? " (Default)" : "";
                    return $"[{kind}] {d.DisplayName}{isDefault}";
                })
                .ToList();
        }

        public static IEnumerable<string> ListRunningApps()
        {
            var tcs = new TaskCompletionSource<List<string>>();

            var thread = new Thread(() =>
            {
                var dispatcher = Dispatcher.CurrentDispatcher;

                try
                {
                    var earAsm = typeof(IAudioDeviceManager).Assembly;

                    var mgrType = earAsm
                        .GetTypes()
                        .FirstOrDefault(t =>
                            t.IsClass &&
                            !t.IsAbstract &&
                            typeof(IAudioDeviceManager).IsAssignableFrom(t) &&
                            t.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                             .Any(c =>
                             {
                                 var ps = c.GetParameters();
                                 return ps.Length == 1 && ps[0].ParameterType.IsEnum;
                             }));

                    if (mgrType == null)
                        throw new TypeLoadException("Could not locate a concrete IAudioDeviceManager with a single enum constructor.");

                    var ctor = mgrType
                        .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        .First(c =>
                        {
                            var ps = c.GetParameters();
                            return ps.Length == 1 && ps[0].ParameterType.IsEnum;
                        });

                    var enumType = ctor.GetParameters()[0].ParameterType;
                    var enumValues = Enum.GetValues(enumType).Cast<object>().ToList();

                    var managers = enumValues
                        .Select(val => (IAudioDeviceManager)ctor.Invoke(new object[] { val }))
                        .ToList();

                    int loadedCount = 0;
                    var frame = new DispatcherFrame();

                    EventHandler onLoaded = (s, e) =>
                    {
                        loadedCount++;
                        if (loadedCount >= managers.Count)
                        {
                            var exes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                            foreach (var mgr in managers)
                            {
                                var isPlayback = string.Equals(mgr.Kind, "Playback", StringComparison.OrdinalIgnoreCase);
                                if (!isPlayback) continue;

                                foreach (var device in mgr.Devices)
                                {
                                    foreach (var sess in device.Groups)
                                    {
                                        var exe = GetExeNameFromSession(sess);
                                        if (!string.IsNullOrWhiteSpace(exe))
                                        {
                                            exes.Add(exe);
                                        }
                                    }
                                }
                            }

                            var list = exes.OrderBy(x => x).ToList();
                            tcs.TrySetResult(list);
                            frame.Continue = false;
                        }
                    };

                    foreach (var mgr in managers)
                    {
                        mgr.Loaded += onLoaded;
                    }

                    Dispatcher.PushFrame(frame);

                    dispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                    dispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();

            return tcs.Task.GetAwaiter().GetResult();
        }

        private static string TryGetProcessPath(int pid)
        {
            try
            {
                using (var p = Process.GetProcessById(pid))
                {
                    return p?.MainModule?.FileName ?? string.Empty;
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string GetExeNameFromSession(object sess)
        {
            var exe = GetProp<string>(sess, "ExeName");
            if (!string.IsNullOrWhiteSpace(exe)) return System.IO.Path.GetFileName(exe);

            var appId = GetProp<string>(sess, "AppId");
            if (!string.IsNullOrWhiteSpace(appId)) return System.IO.Path.GetFileName(appId);

            var display = GetProp<string>(sess, "DisplayName");
            if (!string.IsNullOrWhiteSpace(display)) return System.IO.Path.GetFileName(display);

            var isSystem = GetProp<bool?>(sess, "IsSystemSoundsSession") ?? false;
            if (isSystem) return "System Sounds";

            return null;
        }

        private static string ExtractFileName(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            s = s.Trim();

            var sepIdx = Math.Max(s.LastIndexOf('\\'), s.LastIndexOf('/'));
            if (sepIdx >= 0 && sepIdx + 1 < s.Length)
                s = s.Substring(sepIdx + 1);

            var bang = s.IndexOf('!');
            if (bang > 0) s = s.Substring(0, bang);

            return s.Length > 0 ? s : null;
        }

        private static IAudioDevice FindDeviceByDisplayName(IAudioDeviceManager mgr, string name, out string error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(name))
            {
                error = "Device name is empty.";
                return null;
            }

            var devices = mgr.Devices.ToList();
            var exact = devices.FirstOrDefault(d => string.Equals(d.DisplayName, name, StringComparison.OrdinalIgnoreCase));
            if (exact != null) return exact;

            var contains = devices.Where(d => d.DisplayName.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            if (contains.Count == 1) return contains[0];

            if (contains.Count == 0)
            {
                error = $"Device \"{name}\" not found. Available playback devices:\n  - " +
                        string.Join("\n  - ", devices.Select(d => d.DisplayName));
            }
            else
            {
                error = $"Device name \"{name}\" is ambiguous. Candidates:\n  - " +
                        string.Join("\n  - ", contains.Select(d => d.DisplayName));
            }
            return null;
        }

        private static bool MatchesExe(string exeName, string exeBase)
        {
            if (string.IsNullOrWhiteSpace(exeName) || string.IsNullOrWhiteSpace(exeBase)) return false;
            var baseName = NormalizeExeBase(exeName);
            return string.Equals(baseName, exeBase, StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeExeBase(string exe)
        {
            if (string.IsNullOrWhiteSpace(exe)) return null;
            exe = System.IO.Path.GetFileName(exe.Trim());
            if (exe.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                exe = exe.Substring(0, exe.Length - 4);
            return exe;
        }

        private static T GetProp<T>(object obj, string prop)
        {
            var pi = obj.GetType().GetProperty(prop, BindingFlags.Public | BindingFlags.Instance);
            if (pi == null) return default;
            var val = pi.GetValue(obj);
            if (val == null) return default;
            if (typeof(T).IsAssignableFrom(val.GetType())) return (T)val;
            try { return (T)Convert.ChangeType(val, typeof(T)); } catch { return default; }
        }


        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        private const uint KEYEVENTF_KEYUP = 0x0002;

        private static void ForceAudioSessionRestart(HashSet<int> pids, Action<string> V)
        {
            foreach (var pid in pids)
            {
                try
                {
                    var sessions = GetAudioSessionsForProcess(pid);
                    foreach (var session in sessions)
                    {
                        var volume = session.GetMasterVolume();
                        var muted = session.GetMute();

                        session.SetMute(!muted, Guid.Empty);
                        Thread.Sleep(50);
                        session.SetMute(muted, Guid.Empty);

                        V($"Toggled mute for pid {pid} to force stream restart");
                    }
                }
                catch {  }
            }
        }

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, IntPtr dwExtraInfo);

        private const byte VK_MEDIA_PLAY_PAUSE = 0xB3;
        private const uint KEYEVENTF_EXTENDEDKEY = 0x0001;

        private static IEnumerable<dynamic> GetAudioSessionsForProcess(int pid)
        {
            return new List<dynamic>();
        }

        private static string GetSessionDeviceIdForPid(int pid)
        {
            try
            {
                Type mgrType = null;
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    mgrType = asm.GetType("EarTrumpet.DataModel.WindowsAudio.Internal.AudioDeviceManager", throwOnError: false);
                    if (mgrType != null) break;
                }
                if (mgrType == null) return string.Empty;

                var create = mgrType.GetMethod("Create", BindingFlags.Public | BindingFlags.Static);
                var mgr = create?.Invoke(null, null);
                if (mgr == null) return string.Empty;

                var sessionsProp = mgrType.GetProperty("Sessions", BindingFlags.Public | BindingFlags.Instance);
                var sessions = sessionsProp?.GetValue(mgr) as System.Collections.IEnumerable;
                if (sessions == null) return string.Empty;

                foreach (var s in sessions)
                {
                    var spid = GetProp<int>(s, "ProcessId");
                    if (spid == pid)
                    {
                        return GetProp<string>(s, "DeviceId") ?? string.Empty;
                    }
                }
            }
            catch            {            }
            return string.Empty;
        }

    }

}