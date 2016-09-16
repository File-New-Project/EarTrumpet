using System;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Microsoft.Services.Store.Engagement;

namespace EarTrumpet.BackgroundTasks
{
    public sealed class FeedbackTask : IBackgroundTask
    {
        private BackgroundTaskDeferral _deferral;
        private AppServiceConnection _connection;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();
            taskInstance.Canceled += OnTaskCanceled;

            var details = taskInstance.TriggerDetails as AppServiceTriggerDetails;
            if (details != null)
            {
                _connection = details.AppServiceConnection;
                _connection.RequestReceived += OnRequestReceived;
            }
        }

        private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            if (_deferral != null)
            {
                _deferral.Complete();
            }
        }

        private async void OnRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            var _requestDeferral = args.GetDeferral();

            await StoreServicesFeedbackLauncher.GetDefault().LaunchAsync();

            _requestDeferral.Complete();
        }
    }
}
