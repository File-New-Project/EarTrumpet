using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EarTrumpet.Views
{
    public class VolumeSlider : Slider
    {
        public float PeakValue
        {
            get { return (float)this.GetValue(PeakValueProperty); }
            set { this.SetValue(PeakValueProperty, value); }
        }
        public static readonly DependencyProperty PeakValueProperty = DependencyProperty.Register(
          "PeakValue", typeof(float), typeof(VolumeSlider), new PropertyMetadata(0f, new PropertyChangedCallback(PeakValueChanged)));

        private double _thumbWidth;

        private Border PeakMeter => (Border)GetTemplateChild("PeakMeter");

        public VolumeSlider() : base()
        {
            PreviewTouchDown += OnTouchDown;
            PreviewMouseDown += OnMouseDown;
            TouchUp += OnTouchUp;
            MouseUp += OnMouseUp;
            TouchMove += OnTouchMove;
            MouseMove += OnMouseMove;
            MouseWheel += OnMouseWheel;

            _thumbWidth = (double)TryFindResource("SliderThumbWidth");
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

        private void SizeOrVolumeOrPeakValueChanged()
        {
            var meter = PeakMeter;
            if (meter != null)
            {
                meter.Width = Math.Max(0, (ActualWidth - _thumbWidth) * PeakValue * (Value / 100f));
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
                VisualStateManager.GoToState((FrameworkElement)sender, "Pressed", true);

                SetPositionByControlPoint(e.GetPosition(this));
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
                Rect rcSlider = new Rect(0, 0, ActualWidth, ActualHeight);
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
            if (IsMouseCaptured)
            {
                SetPositionByControlPoint(e.GetPosition(this));
            }
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var amount = Math.Sign(e.Delta) * 2.0;
            ChangePositionByAmount(amount);
            e.Handled = true;
        }

        public void SetPositionByControlPoint(Point point)
        {
            var percent = point.X / ActualWidth;
            Value = Bound((Maximum - Minimum) * percent);
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
}
