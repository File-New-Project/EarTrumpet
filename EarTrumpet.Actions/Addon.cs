using EarTrumpet.Extensibility;
using EarTrumpet.Extensibility.Shared;
using EarTrumpet.Extensions;
using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.Services;
using EarTrumpet.UI.ViewModels;
using EarTrumpet.UI.Views;
using EarTrumpet_Actions.DataModel;
using EarTrumpet_Actions.ViewModel;
using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Windows;

namespace EarTrumpet_Actions
{
    [Export(typeof(IAddonLifecycle))]
    public class Addon : IAddonLifecycle
    {
        public static Addon Current { get; private set; }
        public ActionsManager Manager { get; private set; }
        public ProcessWatcher ProcessWatcher { get; private set; }

        private SettingsWindow _openSettingsWindow;

        public void OnApplicationLifecycleEvent(ApplicationLifecycleEvent evt)
        {
            if (evt == ApplicationLifecycleEvent.Startup2)
            {
                Current = this;
                Manager = new ActionsManager();
                ProcessWatcher = new ProcessWatcher();
                Manager.OnStartup();
            }
            else if (evt == ApplicationLifecycleEvent.Shutdown)
            {
                Manager.OnShuttingDown();
            }
        }

        private void ViewModel_RequestApplyChanges(EarTrumpetAction[] newActions)
        {
            Manager.Apply(newActions);
        }

        public void OpenSettingsWindow()
        {
            if (_openSettingsWindow != null)
            {
                _openSettingsWindow.RaiseWindow();
            }
            else
            {
                ResourceLoader.Load("EarTrumpet-Actions");

                var viewModel = new ActionsEditorViewModel();
                viewModel.RequestApplyChanges += ViewModel_RequestApplyChanges;
                viewModel.RequestHotkey += ViewModel_RequestHotkey;

                _openSettingsWindow = new SettingsWindow();
                _openSettingsWindow.DataContext = viewModel;
                _openSettingsWindow.Closing += (_, __) => _openSettingsWindow = null;
                _openSettingsWindow.Show();
                WindowAnimationLibrary.BeginWindowEntranceAnimation(_openSettingsWindow, () => { });
            }
        }

        private HotkeyData ViewModel_RequestHotkey(HotkeyData currentHotkey)
        {
            Trace.WriteLine("EarTrumpet_Actions.Addon ViewModel_RequestHotkey");

            bool userSaved = false;
            var win = new DialogWindow { Owner = _openSettingsWindow };
            var w = new HotkeySelectViewModel
            {
                Save = new RelayCommand(() =>
                {
                    userSaved = true;
                    win.Close();
                })
            };
            win.DataContext = w;
            win.PreviewKeyDown += w.Window_PreviewKeyDown;
            win.ShowDialog();

            if (userSaved)
            {
                return w.Hotkey;
            }
            return currentHotkey;
        }
    }
}