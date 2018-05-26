using System.Diagnostics;

namespace EarTrumpet.Services
{
    class FeedbackService
    {
        public static void OpenFeedbackHub()
        {
            using (Process.Start("windows-feedback:///?appid=40459File-New-Project.EarTrumpet_1sdd7yawvg6ne!EarTrumpet"))
            {

            }
        }
    }
}
