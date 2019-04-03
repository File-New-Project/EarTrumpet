using EarTrumpet.UI.Helpers;
using System;

namespace EarTrumpet.UI.ViewModels
{
    public interface ISettingsViewModel
    {
        void ShowDialog(string title, string description, string btn1, string btn2, Action btn1Clicked, Action btn2Clicked);
        void CompleteNavigation(NavigationCookie cookie);
    }
}