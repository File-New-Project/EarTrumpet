using System.Windows;
using System.Windows.Controls;

namespace EarTrumpet.UI.Behaviors
{
    public class TextBoxEx
    {
        // ClearText: Clear TextBox if set to True.
        public static bool GetClearText(DependencyObject obj) => (bool)obj.GetValue(ClearTextProperty);
        public static void SetClearText(DependencyObject obj, bool value) => obj.SetValue(ClearTextProperty, value);
        public static readonly DependencyProperty ClearTextProperty =
        DependencyProperty.RegisterAttached("ClearText", typeof(bool), typeof(TextBoxEx), new PropertyMetadata(false, ClearTextChanged));

        private static void ClearTextChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == true)
            {
                ((TextBox)dependencyObject).Text = "";
            }
        }
    }
}
