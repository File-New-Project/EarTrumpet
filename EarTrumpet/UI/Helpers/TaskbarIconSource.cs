using EarTrumpet.Interop.Helpers;
using System;
using System.Diagnostics;
using System.Drawing;

namespace EarTrumpet.UI.Helpers
{
    public class TaskbarIconSource
    {
        public event Action<TaskbarIconSource> Changed;

        public Icon Current { get; private set; }
        public object Tag { get; set; }

        public string Source
        {
            get => _path;
            set
            {
                if (_path != value)
                {
                    _path = value;
                    CheckForUpdate();
                }
            }
        }

        private readonly Func<Icon, Icon> _processIcon;
        private readonly Func<string> _getConfiguration;
        private string _path;
        private string _configuration;

        public TaskbarIconSource(Func<Icon, Icon> processIcon, Func<string> getPropertiesConfiguration)
        {
            _processIcon = processIcon;
            _getConfiguration = getPropertiesConfiguration;
        }

        public void CheckForUpdate()
        {
            var nextConfiguation = _path + ":" + _getConfiguration();
            if (nextConfiguation != _configuration)
            {
                _configuration = nextConfiguation;
                Trace.WriteLine($"TaskbarIconSource Changed: {nextConfiguation}");
                Current?.Dispose();

                // Continue to _processIcon even if we have a null icon, to support fallback scenarios.
                Icon nextIcon = null;
                try
                {
                    nextIcon = IconHelper.LoadIconForTaskbar(_path);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"TaskbarIconSource CheckForUpdate Changed Failed: {ex}");
                }
                Current = _processIcon.Invoke(nextIcon);
                Changed?.Invoke(this);
            }
        }
    }
}
