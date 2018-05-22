using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.Foundation;

namespace EarTrumpet.Services
{
    class FeedbackService
    {
        private static AppServiceConnection _appServiceConnection;

        private static void AppServiceConnectionCompleted(IAsyncOperation<AppServiceConnectionStatus> operation, AsyncStatus asyncStatus)
        {
            var status = operation.GetResults();
            if (status == AppServiceConnectionStatus.Success)
            {
                var secondOperation = _appServiceConnection.SendMessageAsync(null);
                secondOperation.Completed = (_, __) =>
                {
                    _appServiceConnection.Dispose();
                    _appServiceConnection = null;
                    App.Current.Exit -= CloseFeedbackServiceAtExit;
                };
            }
        }

        public static void StartAppServiceAndFeedbackHub()
        {
            if (_appServiceConnection == null)
            {
                _appServiceConnection = new AppServiceConnection();
                App.Current.Exit += CloseFeedbackServiceAtExit;
            }

            _appServiceConnection.AppServiceName = "SendFeedback";
            _appServiceConnection.PackageFamilyName = Package.Current.Id.FamilyName;
            _appServiceConnection.OpenAsync().Completed = AppServiceConnectionCompleted;
        }

        private static void CloseFeedbackServiceAtExit(object sender, System.Windows.ExitEventArgs e)
        {
            CloseAppService();
        }

        public static void CloseAppService()
        {
            if (_appServiceConnection != null)
            {
                _appServiceConnection.Dispose();
            }
        }
    }
}
