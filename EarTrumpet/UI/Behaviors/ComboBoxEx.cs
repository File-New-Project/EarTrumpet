using EarTrumpet.Extensions;
using EarTrumpet.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace EarTrumpet.UI.Behaviors
{
    public class ComboBoxEx
    {
        private static readonly int MaxSearchBoxResultItems = 5;

        // Since we don't use SelectionChanged, this handler makes item click work using mouse events.
        public static bool GetItemClickEnabled(DependencyObject obj) => (bool)obj.GetValue(ItemClickEnabledProperty);
        public static void SetItemClickEnabled(DependencyObject obj, bool value) => obj.SetValue(ItemClickEnabledProperty, value);
        public static readonly DependencyProperty ItemClickEnabledProperty =
        DependencyProperty.RegisterAttached("ItemClickEnabled", typeof(bool), typeof(ComboBoxEx), new PropertyMetadata(false, ItemClickEnabledChanged));

        private static void ItemClickEnabledChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            Debug.Assert((bool)e.NewValue == true);
            var comboBoxItem = ((ComboBoxItem)dependencyObject);
            comboBoxItem.PreviewMouseLeftButtonDown += OnComboBoxItemPreviewMouseLeftButtonDown; ;
        }

        private static void OnComboBoxItemPreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var comboBoxItem = (ComboBoxItem)sender;
            var comboBox = comboBoxItem.FindVisualParent<ComboBox>();
            var item = (SettingsSearchItemViewModel)comboBoxItem.DataContext;
            InvokeSearchItem(item, comboBox);
        }

        public static object GetSearchItemsSource(DependencyObject obj) => (object)obj.GetValue(SearchItemsSourceProperty);
        public static void SetSearchItemsSource(DependencyObject obj, object value) => obj.SetValue(SearchItemsSourceProperty, value);
        public static readonly DependencyProperty SearchItemsSourceProperty =
        DependencyProperty.RegisterAttached("SearchItemsSource", typeof(object), typeof(ComboBoxEx), new PropertyMetadata(null, SearchItemsSourceChanged));

        private static void SearchItemsSourceChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var comboBox = ((ComboBox)dependencyObject);
            comboBox.AddHandler(TextBox.TextChangedEvent, new TextChangedEventHandler(OnTextChanged));
            comboBox.PreviewKeyUp += OnPreviewKeyUp;
        }

        private static void OnPreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var comboBox = (ComboBox)sender;
            var textBox = comboBox.FindVisualChild<TextBox>();

            if (e.Key == System.Windows.Input.Key.Enter)
            {
                var item = (SettingsSearchItemViewModel)comboBox.SelectedItem;
                if (item == null)
                {
                    item = DoSearch((SettingsViewModel)GetSearchItemsSource(comboBox), textBox.Text).FirstOrDefault();
                }

                InvokeSearchItem(item, comboBox);
            }
        }

        private static void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var comboBox = (ComboBox)sender;
            var textBox = comboBox.FindVisualChild<TextBox>();

            comboBox.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (!string.IsNullOrWhiteSpace(textBox.Text))
                {
                    comboBox.ItemsSource = DoSearch((SettingsViewModel)GetSearchItemsSource(comboBox), textBox.Text);
                    comboBox.IsDropDownOpen = true;
                }
                else
                {
                    ClearComboBox(comboBox);
                }
            }));
        }

        private static IEnumerable<SettingsSearchItemViewModel> DoSearch(SettingsViewModel viewModel, string text)
        {
            var ret = new List<SettingsSearchItemViewModel>();
            text = text.ToLower();

            foreach (var cat in viewModel.Categories)
            {
                foreach (var page in cat.Pages)
                {
                    if (page.Title.ToLower().Contains(text))
                    {
                        ret.Add(new SettingsSearchItemViewModel
                        {
                            DisplayName = page.Title,
                            Glyph = page.Glyph,
                            Invoke = () => viewModel.InvokeSearchResult(cat, page),
                            SearchText = text,
                        });
                    }
                }

                if (ret.Count > MaxSearchBoxResultItems)
                {
                    return ret;
                }
            }

            if (ret.Count == 0)
            {
                ret.Add(new SettingsSearchItemViewModel
                {
                    DisplayName = Properties.Resources.SearchBoxNoResultsText,
                    Invoke = () => { },
                });
            }
            return ret;
        }

        private static void InvokeSearchItem(SettingsSearchItemViewModel item, ComboBox comboBox)
        {
            ClearComboBox(comboBox);

            if (item != null)
            {
                item.Invoke();
            }
        }

        private static void ClearComboBox(ComboBox comboBox)
        {
            comboBox.Text = null;
            comboBox.ItemsSource = null;
            comboBox.IsDropDownOpen = false;
        }
    }
}
