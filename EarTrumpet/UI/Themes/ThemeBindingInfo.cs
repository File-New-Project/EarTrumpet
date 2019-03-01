using System;
using System.Windows;

namespace EarTrumpet.UI.Themes
{
    class ThemeBindingInfo<T>
    {
        private readonly string _propertyName;
        private readonly string _value;
        private readonly Func<DependencyObject, string, T> _applyCallback;
        private WeakReference<DependencyObject> _element;
        private bool _isAttached;
        private T _initialValue;

        public ThemeBindingInfo(DependencyObject element, string value, string propertyName, Func<DependencyObject, string, T> applyCallback)
        {
            _propertyName = propertyName;
            _element = new WeakReference<DependencyObject>(element);
            _value = value;
            _applyCallback = applyCallback;

            SetPropertyToDesiredValue(element);

            if (element is FrameworkElement)
            {
                ((FrameworkElement)element).Loaded += Element_Loaded;
            }
            else if (element is FrameworkContentElement)
            {
                ((FrameworkContentElement)element).Loaded += Element_Loaded;
            }
        }

        public void Leaving()
        {
            if (_element.TryGetTarget(out var element))
            {
                if (element is FrameworkContentElement)
                {
                    ((FrameworkContentElement)element).Loaded -= Element_Loaded;
                }
                else if (element is FrameworkElement)
                {
                    ((FrameworkElement)element).Loaded -= Element_Loaded;
                }

                if (_isAttached)
                {
                    WriteProperty(element, _initialValue);
                    Manager.Current.ThemeChanged -= ThemeChanged;
                }
            }

            _element = null;
        }

        private void Element_Loaded(object sender, RoutedEventArgs e)
        {
            if (_element.TryGetTarget(out var element))
            {
                if (element is FrameworkContentElement)
                {
                    ((FrameworkContentElement)element).Loaded -= Element_Loaded;
                }
                else if (element is FrameworkElement)
                {
                    ((FrameworkElement)element).Loaded -= Element_Loaded;
                }

                SetPropertyToDesiredValue(element);
            }
        }

        public bool SetPropertyToDesiredValue(DependencyObject element)
        {
            var type = Options.GetSource(element);
            if (type == null)
            {
                return false;
            }

            _initialValue = (T)ReadProperty(element);
            _isAttached = true;
            WriteProperty(element, _applyCallback.Invoke(element, _value));
            Manager.Current.ThemeChanged += ThemeChanged;
            return true;
        }

        private void ThemeChanged()
        {
            if (_element.TryGetTarget(out var element))
            {
                WriteProperty(element, _applyCallback.Invoke(element, _value));
            }
        }

        object ReadProperty(DependencyObject element)
        {
            var prop = element.GetType().GetProperty(_propertyName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            return prop.GetValue(element);
        }

        void WriteProperty(DependencyObject element, object value)
        {
            if (element == null)
            {
                return;
            }
            var prop = element.GetType().GetProperty(_propertyName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            prop.SetValue(element, value);
        }
    }
}
