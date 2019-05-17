using EarTrumpet.Extensions;
using System;
using System.Windows;

namespace EarTrumpet.UI.Helpers
{
    public class WindowHolder
    {
        Func<Window> _create;
        Window _openWindow;

        public WindowHolder(Func<Window> create)
        {
            _create = create;
        }

        public void OpenOrClose()
        {
            if (_openWindow == null)
            {
                CreateWindow();
            }
            else
            {
                _openWindow.Close();
                _openWindow = null;
            }
        }

        public void OpenOrBringToFront()
        {
            if (_openWindow == null)
            {
                CreateWindow();
            }
            else
            {
                _openWindow.RaiseWindow();
            }
        }

        private void CreateWindow()
        {
            _openWindow = _create();
            _openWindow.Show();
            WindowAnimationLibrary.BeginWindowEntranceAnimation(_openWindow, () => { });
        }

        public void Destroyed()
        {
            _openWindow = null;
        }
    }
}
