using EarTrumpet.UI.Helpers;
using System.Diagnostics;

namespace EarTrumpet.UI.Services
{
    class FeedbackService
    {
        public static void OpenFeedbackHub()
        {
            Trace.WriteLine($"FeedbackService OpenFeedbackHub");

            using (ProcessHelper.StartNoThrow("windows-feedback:///?appid=40459File-New-Project.EarTrumpet_1sdd7yawvg6ne!EarTrumpet")) { }
        }
    }
}
