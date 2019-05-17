using EarTrumpet.Interop.Helpers;
using System;
using System.Diagnostics;
using System.Drawing;

namespace EarTrumpet.UI
{
    public class IconSource
    {
        public event Action<IconSource> Changed;

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

        public IconSource(Func<Icon, Icon> processIcon, Func<string> getPropertiesConfiguration)
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
                Trace.WriteLine($"IconSource IconResolutionPropertiesChanged: {nextConfiguation}");
                Current?.Dispose();
                Current = _processIcon.Invoke(IconHelper.LoadSmallIcon(_path));
                Changed?.Invoke(this);
            }
        }
    }
}
