using EarTrumpet.Extensions;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EarTrumpet.UserControls
{
    public class VolumeSlider : Slider
    {
        public VolumeSlider() : base()
        {
            TouchDown += OnTouchDown;
            MouseDown += OnMouseDown;
            TouchUp += OnTouchUp;
            MouseUp += OnMouseUp;
            TouchMove += OnTouchMove;
            MouseMove += OnMouseMove;
            MouseWheel += OnMouseWheel;
        }

        private void OnTouchDown(object sender, TouchEventArgs e)
        {
            VisualStateManager.GoToState((FrameworkElement)sender, "Pressed", true);

            var slider = (Slider)sender;
            slider.SetPositionByControlPoint(e.GetTouchPoint(slider).Position);
            slider.CaptureTouch(e.TouchDevice);

            e.Handled = true;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                VisualStateManager.GoToState((FrameworkElement)sender, "Pressed", true);

                var slider = (Slider)sender;
                slider.SetPositionByControlPoint(e.GetPosition(slider));
                slider.CaptureMouse();

                e.Handled = true;
            }
        }

        private void OnTouchUp(object sender, TouchEventArgs e)
        {
            VisualStateManager.GoToState((FrameworkElement)sender, "Normal", true);

            var slider = (Slider)sender;
            slider.ReleaseTouchCapture(e.TouchDevice);
            e.Handled = true;
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            var slider = (Slider)sender;
            if (slider.IsMouseCaptured)
            {
                // If the point is outside of the control, clear the hover state.
                Rect rcSlider = new Rect(0, 0, slider.ActualWidth, slider.ActualHeight);
                if (!rcSlider.Contains(e.GetPosition(slider)))
                {
                    VisualStateManager.GoToState((FrameworkElement)sender, "Normal", true);
                }

                ((Slider)sender).ReleaseMouseCapture();
                e.Handled = true;
            }
        }

        private void OnTouchMove(object sender, TouchEventArgs e)
        {
            var slider = (Slider)sender;
            if (slider.AreAnyTouchesCaptured)
            {
                slider.SetPositionByControlPoint(e.GetTouchPoint(slider).Position);
                e.Handled = true;
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            var slider = (Slider)sender;
            if (slider.IsMouseCaptured)
            {
                slider.SetPositionByControlPoint(e.GetPosition(slider));
            }
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var slider = (Slider)sender;
            var amount = Math.Sign(e.Delta) * 2.0;
            slider.ChangePositionByAmount(amount);
            e.Handled = true;
        }
    }
}
