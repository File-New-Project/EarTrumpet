using EarTrumpet.DataModel;
using EarTrumpet.Extensions;
using EarTrumpet.UI;
using System.Linq;
using System.Windows.Media;

namespace EarTrumpet_Actions.ViewModel
{
    class AppViewModel : AppViewModelBase
    {
        public IconLoadInfo Icon { get; }
        public bool IsDesktopApp { get; }
        public Color Background => _app.IsDesktopApp ? Colors.Transparent : _app.BackgroundColor.ToABGRColor();
        public char IconText => string.IsNullOrWhiteSpace(DisplayName) ? '?' : DisplayName.ToUpperInvariant().FirstOrDefault(x => char.IsLetterOrDigit(x));

        private IAudioDeviceSession _app;

        public AppViewModel(IAudioDeviceSession app)
        {
            _app = app;
            DisplayName = app.SessionDisplayName;
            IsDesktopApp = app.IsDesktopApp;
            Id = app.Id;
            Icon = new IconLoadInfo { IconPath = app.IconPath, IsDesktopApp = app.IsDesktopApp };
        }
    }
}
