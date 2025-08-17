﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace EarTrumpet.UI.Controls;

public class VolumeSlider : Slider
{
    public float PeakValue1
    {
        get { return (float)this.GetValue(PeakValue1Property); }
        set { this.SetValue(PeakValue1Property, value); }
    }
    public static readonly DependencyProperty PeakValue1Property = DependencyProperty.Register(
      "PeakValue1", typeof(float), typeof(VolumeSlider), new PropertyMetadata(0f, new PropertyChangedCallback(PeakValueChanged)));

    public float PeakValue2
    {
        get { return (float)this.GetValue(PeakValue2Property); }
        set { this.SetValue(PeakValue2Property, value); }
    }
    public static readonly DependencyProperty PeakValue2Property = DependencyProperty.Register(
      "PeakValue2", typeof(float), typeof(VolumeSlider), new PropertyMetadata(0f, new PropertyChangedCallback(PeakValueChanged)));

    private Border _peakMeter1;
    private Border _peakMeter2;
    private Thumb _thumb;
    private Point _lastMousePosition;

    public VolumeSlider() : base()
    {
        PreviewTouchDown += OnTouchDown;
        PreviewMouseDown += OnMouseDown;
        TouchUp += OnTouchUp;
        MouseUp += OnMouseUp;
        TouchMove += OnTouchMove;
        MouseMove += OnMouseMove;
        MouseWheel += OnMouseWheel;
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;

        UseLogarithmicVolumeChangedHandler(null, new EventArgs());
        App.Settings.UseLogarithmicVolumeChanged += UseLogarithmicVolumeChangedHandler;
    }

    private void UseLogarithmicVolumeChangedHandler(object sender, EventArgs e)
    {
        UpdateVolumeRange();
        SizeOrVolumeOrPeakValueChanged();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _thumb = (Thumb)GetTemplateChild("SliderThumb");
        _peakMeter1 = (Border)GetTemplateChild("PeakMeter1");
        _peakMeter2 = (Border)GetTemplateChild("PeakMeter2");
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        App.Settings.UseLogarithmicVolumeChanged -= UseLogarithmicVolumeChangedHandler;
    }

    protected override Size ArrangeOverride(Size arrangeBounds)
    {
        var ret = base.ArrangeOverride(arrangeBounds);
        SizeOrVolumeOrPeakValueChanged();
        return ret;
    }

    private static void PeakValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((VolumeSlider)d).SizeOrVolumeOrPeakValueChanged();
    }

    private void UpdateVolumeRange()
    {
        if (App.Settings.UseLogarithmicVolume)
        {
            Minimum = App.Settings.LogarithmicVolumeMinDb;
            Maximum = 0f;
            TickFrequency = 0.1;
        }
        else
        {
            Minimum = 0f;
            Maximum = 100f;
            TickFrequency = 1;
        }
    }

    private void SizeOrVolumeOrPeakValueChanged()
    {
        if (_peakMeter1 != null)
        {
            if (App.Settings.UseLogarithmicVolume)
            {
                _peakMeter1.Width = Math.Max(0, (ActualWidth - _thumb.ActualWidth) * (PeakValue1 - Value / Minimum));
            }
            else
            {
                _peakMeter1.Width = Math.Max(0, (ActualWidth - _thumb.ActualWidth) * PeakValue1 * (Value / 100f));
            }
        }

        if (_peakMeter2 != null)
        {
            if (App.Settings.UseLogarithmicVolume)
            {
                _peakMeter2.Width = Math.Max(0, (ActualWidth - _thumb.ActualWidth) * (PeakValue2 - Value / Minimum));
            }
            else
            {
                _peakMeter2.Width = Math.Max(0, (ActualWidth - _thumb.ActualWidth) * PeakValue2 * (Value / 100f));
            }
        }
    }

    private void OnTouchDown(object sender, TouchEventArgs e)
    {
        VisualStateManager.GoToState((FrameworkElement)sender, "Pressed", true);

        SetPositionByControlPoint(e.GetTouchPoint(this).Position);
        CaptureTouch(e.TouchDevice);

        e.Handled = true;
    }

    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            _lastMousePosition = e.GetPosition(this);
            VisualStateManager.GoToState((FrameworkElement)sender, "Pressed", true);

            if (!_thumb.IsMouseOver)
            {
                SetPositionByControlPoint(_lastMousePosition);
            }

            CaptureMouse();
            e.Handled = true;
        }
    }

    private void OnTouchUp(object sender, TouchEventArgs e)
    {
        VisualStateManager.GoToState((FrameworkElement)sender, "Normal", true);

        ReleaseTouchCapture(e.TouchDevice);
        e.Handled = true;
    }

    private void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (IsMouseCaptured)
        {
            // If the point is outside of the control, clear the hover state.
            var rcSlider = new Rect(0, 0, ActualWidth, ActualHeight);
            if (!rcSlider.Contains(e.GetPosition(this)))
            {
                VisualStateManager.GoToState((FrameworkElement)sender, "Normal", true);
            }

            ReleaseMouseCapture();
            e.Handled = true;
        }
    }

    private void OnTouchMove(object sender, TouchEventArgs e)
    {
        if (AreAnyTouchesCaptured)
        {
            SetPositionByControlPoint(e.GetTouchPoint(this).Position);
            e.Handled = true;
        }
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        var mousePosition = e.GetPosition(this);
        if (IsMouseCaptured && mousePosition != _lastMousePosition)
        {
            _lastMousePosition = mousePosition;
            SetPositionByControlPoint(e.GetPosition(this));
        }
    }

    private void OnMouseWheel(object sender, MouseWheelEventArgs e)
    {
        var amount = Math.Sign(e.Delta) * (App.Settings.UseLogarithmicVolume ? 0.2 : 2.0);
        ChangePositionByAmount(amount);
        e.Handled = true;
    }

    public void SetPositionByControlPoint(Point point)
    {
        var percent = point.X / ActualWidth;
        Value = App.Settings.UseLogarithmicVolume
            ? Math.Round((percent - 1) * -App.Settings.LogarithmicVolumeMinDb, 1)
            : Bound((Maximum - Minimum) * percent);
    }

    public void ChangePositionByAmount(double amount)
    {
        Value = Bound(Value + amount);
    }

    public double Bound(double val)
    {
        return Math.Max(Minimum, Math.Min(Maximum, val));
    }
}
