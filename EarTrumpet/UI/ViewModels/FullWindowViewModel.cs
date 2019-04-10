﻿using System.Collections.ObjectModel;
using System.Windows;

namespace EarTrumpet.UI.ViewModels
{
    public class FullWindowViewModel : BindableBase, IPopupHostViewModel
    {
        public ObservableCollection<DeviceViewModel> AllDevices => _mainViewModel.AllDevices;
        public ModalDialogViewModel Dialog { get; }
        private DeviceCollectionViewModel _mainViewModel;

        public FullWindowViewModel(DeviceCollectionViewModel mainViewModel)
        {
            Dialog = new ModalDialogViewModel();
            _mainViewModel = mainViewModel;
            _mainViewModel.OnFullWindowOpened();
        }

        public void Close()
        {
            Dialog.IsVisible = false;
            _mainViewModel.OnFullWindowClosed();
        }

        public void OpenPopup(object vm, FrameworkElement container)
        {
            Dialog.IsVisible = false;

            if (vm is IAppItemViewModel)
            {
                Dialog.Focused = new FocusedAppItemViewModel(_mainViewModel, (IAppItemViewModel)vm);
            }
            else
            {
                var deviceViewModel = new FocusedDeviceViewModel(_mainViewModel, (DeviceViewModel)vm);
                if (deviceViewModel.IsApplicable)
                {
                    Dialog.Focused = deviceViewModel;
                }
            }

            if (Dialog.Focused != null)
            {
                Dialog.Focused.RequestClose += () => Dialog.IsVisible = false;
                Dialog.Source = container;
                Dialog.IsVisible = true;
            }
        }
    }
}
