using EarTrumpet.Extensions;
using System.Windows;
using System.Windows.Controls;

namespace EarTrumpet.UI.Behaviors
{
    public class TextBoxEx
    {
        // ClearText: Clear TextBox or the parent ComboBox.
        public static bool GetClearText(DependencyObject obj) => (bool)obj.GetValue(ClearTextProperty);
        public static void SetClearText(DependencyObject obj, bool value) => obj.SetValue(ClearTextProperty, value);
        public static readonly DependencyProperty ClearTextProperty =
        DependencyProperty.RegisterAttached("ClearText", typeof(bool), typeof(TextBoxEx), new PropertyMetadata(false, ClearTextChanged));

        private static void ClearTextChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == true)
            {
                var parent = dependencyObject.FindVisualParent<ComboBox>();
                if (parent != null)
                {
                    parent.Text = "";
                    parent.SelectedItem = null;
                }
                else
                {
                    // Ignore !IsLoaded to cleverly allow IsPressed=False to be our trigger but also
                    // don't clear TextBoxes when they are initially created.
                    var textBox = ((TextBox)dependencyObject);
                    if (textBox.IsLoaded)
                    {
                        textBox.Text = "";
                    }
                }
            }
        }
    }
}
