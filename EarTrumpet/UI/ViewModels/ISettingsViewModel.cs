using System;
using EarTrumpet.UI.Services;

namespace EarTrumpet.UI.ViewModels
{
    public interface ISettingsViewModel
    {
        string Title { get; }
        event Func<HotkeyData, HotkeyData> RequestHotkey;
    }
}