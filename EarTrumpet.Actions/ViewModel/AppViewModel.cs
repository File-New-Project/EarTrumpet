using EarTrumpet.DataModel;
using EarTrumpet.UI;

namespace EarTrumpet_Actions.ViewModel
{
    class AppViewModel : AppViewModelBase
    {
        public IconLoadInfo Icon { get; }
        public bool IsDesktopApp { get; }

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
