using EarTrumpet.Interop.Helpers;
using EarTrumpet.UI.ViewModels;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Automation;
using System.Windows.Forms;

namespace EarTrumpet.Extensions
{
    public class TaskbarMiddleClickMuteService : IDisposable
    {
        private readonly MouseHook _mouseHook;
        private readonly DeviceCollectionViewModel _collectionViewModel;
        private readonly AppSettings _settings;
        private bool _disposed = false;

        public TaskbarMiddleClickMuteService(DeviceCollectionViewModel collectionViewModel, AppSettings settings)
        {
            _collectionViewModel = collectionViewModel;
            _settings = settings;
            _mouseHook = new MouseHook();
            _mouseHook.MiddleClickEvent += OnMiddleClick;
            _mouseHook.SetHook();
        }

        private int OnMiddleClick(object sender, MouseEventArgs e)
        {
            if (!_settings.UseTaskbarMiddleClickMute)
            {
                return 0; 
            }

            try
            {
                if (!IsClickOnTaskbar(e.X, e.Y))
                {
                    return 0;
                }

                System.Threading.Tasks.Task.Run(() => 
                {
                    try
                    {
                        string appName = GetTaskbarButtonAppName(e.X, e.Y);
                        if (!string.IsNullOrEmpty(appName))
                        {
                            ToggleMuteForApp(appName);
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"TaskbarMiddleClickMuteService error: {ex.Message}");
                    }
                });

                return 1;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"TaskbarMiddleClickMuteService OnMiddleClick error: {ex.Message}");
            }

            return 0;
        }

        private bool IsClickOnTaskbar(int x, int y)
        {
            try
            {
                var taskbarState = WindowsTaskbar.Current;
                var point = new System.Drawing.Point(x, y);
                
                var rect = taskbarState.Size;
                var bounds = new System.Drawing.Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
                
                if (bounds.Contains(point))
                {
                    return true;
                }
            }
            catch { }
            return false;
        }

        private string GetTaskbarButtonAppName(int x, int y)
        {
            try
            {
                var point = new System.Windows.Point(x, y);
                AutomationElement element = AutomationElement.FromPoint(point);

                if (element == null)
                    return null;

                AutomationElement current = element;
                int maxDepth = 10;
                int depth = 0;

                while (current != null && depth < maxDepth)
                {
                    string name = current.Current.Name;
                    string className = current.Current.ClassName;
                    var controlType = current.Current.ControlType;

                    if (!string.IsNullOrEmpty(name) && 
                        (className == "Taskbar.TaskListButtonAutomationPeer" ||
                         className.Contains("TaskListButton") ||
                         controlType == ControlType.Button ||
                         controlType == ControlType.ListItem ||
                         controlType == ControlType.MenuItem)) 
                    {
                        string cleanName = CleanAppName(name);
                        if (!string.IsNullOrEmpty(cleanName))
                        {
                            return cleanName;
                        }
                    }

                    try
                    {
                        TreeWalker walker = TreeWalker.ControlViewWalker;
                        current = walker.GetParent(current);
                        depth++;
                    }
                    catch
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"TaskbarMiddleClickMuteService GetTaskbarButtonAppName error: {ex.Message}");
            }

            return null;
        }

        private string CleanAppName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            string cleanName = name;

            cleanName = System.Text.RegularExpressions.Regex.Replace(cleanName, @"\s*-\s*\d+\s*.*$", "");

            int dashIndex = cleanName.IndexOf(" - ");
            if (dashIndex > 0)
            {
                cleanName = cleanName.Substring(0, dashIndex);
            }

            cleanName = System.Text.RegularExpressions.Regex.Replace(cleanName, @"\s*\(\d+\)\s*$", "");
            
            return cleanName.Trim();
        }

        private bool ToggleMuteForApp(string appName)
        {
            if (string.IsNullOrEmpty(appName))
                return false;

            string lowerAppName = appName.ToLowerInvariant();

            foreach (var device in _collectionViewModel.AllDevices)
            {
                foreach (var app in device.Apps)
                {
                    string displayName = app.DisplayName?.ToLowerInvariant() ?? "";
                    string exeName = app.ExeName?.ToLowerInvariant() ?? "";

                    if (displayName.Contains(lowerAppName) || 
                        lowerAppName.Contains(displayName) ||
                        exeName.Contains(lowerAppName) ||
                        lowerAppName.Contains(exeName.Replace(".exe", "")))
                    {
                        app.IsMuted = !app.IsMuted;
                        Trace.WriteLine($"TaskbarMiddleClickMuteService toggled mute for {app.DisplayName}");
                        return true;
                    }
                }
            }

            return false;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _mouseHook.MiddleClickEvent -= OnMiddleClick;
                    _mouseHook.UnHook();
                }
                _disposed = true;
            }
        }

        ~TaskbarMiddleClickMuteService()
        {
            Dispose(false);
        }
    }
}
