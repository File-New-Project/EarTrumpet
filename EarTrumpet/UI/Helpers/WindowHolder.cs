﻿using EarTrumpet.Extensions;
using System;
using System.Windows;

namespace EarTrumpet.UI.Helpers;

public class WindowHolder(Func<Window> create)
{
    private readonly Func<Window> _create = create;
    private Window _openWindow;

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
        _openWindow.Closed += (_, __) => _openWindow = null;
        _openWindow.Show();
        WindowAnimationLibrary.BeginWindowEntranceAnimation(_openWindow, () => { });
    }
}
