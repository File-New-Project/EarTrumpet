using System;
using System.Windows;
using System.Windows.Threading;

namespace EarTrumpet.Extensions
{
    public static class FrameworkElementExtensions
    {
        public static void WaitForKeyboardVisuals(this FrameworkElement element, Action completed)
        {
            // ApplicationIdle is necessary as KeyboardNavigation/ScheduleCleanup will use ContextIdle to purge the focus visuals.
            // See: https://referencesource.microsoft.com/#PresentationFramework/src/Framework/System/Windows/Input/KeyboardNavigation.cs,3707
            element.Dispatcher.BeginInvoke(completed, DispatcherPriority.ApplicationIdle, null);
        }
    }
}
