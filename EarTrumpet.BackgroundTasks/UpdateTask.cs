using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace EarTrumpet.BackgroundTasks
{
    public sealed class UpdateTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            // your app migration/update code here
            var hasShownFirstRun = true;
            Windows.Storage.ApplicationData.Current.LocalSettings.Values[nameof(hasShownFirstRun)] = true;
        }
    }
}
