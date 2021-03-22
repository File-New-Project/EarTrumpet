using EarTrumpet.Extensions;
using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.ViewModels;
using EarTrumpet.Actions.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;

namespace EarTrumpet.Actions.Controls
{
    public class LinkedTextBlock : TextBlock
    {
        public Popup Popup
        {
            get { return (Popup)this.GetValue(PopupProperty); }
            set { this.SetValue(PopupProperty, value); }
        }
        public static readonly DependencyProperty PopupProperty = DependencyProperty.Register(
          "Popup", typeof(Popup), typeof(LinkedTextBlock), new PropertyMetadata(null));

        // We avoid ContextMenu because something external connects it to right-click behavior that (seemingly) can't be prevented.
        public ContextMenu ContextMenu2
        {
            get { return (ContextMenu)this.GetValue(ContextMenuProperty2); }
            set { this.SetValue(ContextMenuProperty2, value); }
        }
        public static readonly DependencyProperty ContextMenuProperty2 = DependencyProperty.Register(
          "ContextMenu2", typeof(ContextMenu), typeof(LinkedTextBlock), new PropertyMetadata(null));

        public object DataItem
        {
            get { return (object)this.GetValue(DataItemProperty); }
            set { this.SetValue(DataItemProperty, value); }
        }
        public static readonly DependencyProperty DataItemProperty = DependencyProperty.Register(
          "DataItem", typeof(object), typeof(LinkedTextBlock), new PropertyMetadata(null, new PropertyChangedCallback(DataItemChanged)));

        private static void DataItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((LinkedTextBlock)d).DataItemChanged();

        public string FormatText
        {
            get { return (string)this.GetValue(FormatTextProperty); }
            set { this.SetValue(FormatTextProperty, value); }
        }
        public static readonly DependencyProperty FormatTextProperty = DependencyProperty.Register(
          "FormatText", typeof(string), typeof(LinkedTextBlock), new PropertyMetadata("", new PropertyChangedCallback(FormatTextChanged)));

        private static void FormatTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((LinkedTextBlock)d).PropertiesChanged();

        public Style HyperlinkStyle
        {
            get { return (Style)this.GetValue(HyperlinkStyleProperty); }
            set { this.SetValue(HyperlinkStyleProperty, value); }
        }
        public static readonly DependencyProperty HyperlinkStyleProperty = DependencyProperty.Register(
          "HyperlinkStyle", typeof(Style), typeof(LinkedTextBlock), new PropertyMetadata(null, new PropertyChangedCallback(HyperlinkStyleChanged)));

        public Style RunStyle
        {
            get { return (Style)this.GetValue(RunStyleProperty); }
            set { this.SetValue(RunStyleProperty, value); }
        }
        public static readonly DependencyProperty RunStyleProperty = DependencyProperty.Register(
          "RunStyle", typeof(Style), typeof(LinkedTextBlock), new PropertyMetadata(null, new PropertyChangedCallback(RunStyleChanged)));


        private static void HyperlinkStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((LinkedTextBlock)d).PropertiesChanged();
        private static void RunStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((LinkedTextBlock)d).PropertiesChanged();

        private void DataItemChanged()
        {
            ((INotifyPropertyChanged)DataItem).PropertyChanged += (s, e) => PropertiesChanged();
        }

        private void PropertiesChanged()
        {
            this.Inlines.Clear();

            if (ContextMenu != null && Popup != null)
            {
                ContextMenu.Loaded += ContextMenu_Loaded;
                Popup.Loaded += Popup_Loaded;
            }

            ReadLinksAndText(FormatText, (text, isLink) =>
            {
                text = text.Trim();
                if (!isLink)
                {
                    var run = new Run(text);
                    run.Style = RunStyle;
                    this.Inlines.Add(run);
                }
                else
                {
                    var resolvedPropertyObject = DataItem.GetType().GetProperty(text).GetValue(DataItem, null);
                    var link = new Hyperlink(new Run(resolvedPropertyObject.ToString()));
                    link.NavigateUri = new Uri("about:none");
                    link.Style = HyperlinkStyle;

                    link.RequestNavigate += (s, e) =>
                    {
                        // Take focus now so that we get return focus when the user leaves.
                        link.Focus();
                        var dpiX = Window.GetWindow(this).DpiX();
                        var dpiY = Window.GetWindow(this).DpiY();

                        if (resolvedPropertyObject is IOptionViewModel)
                        {
                            ContextMenu2.Opacity = 0;
                            ContextMenu2.ItemsSource = GetContextMenuFromOptionViewModel((IOptionViewModel)resolvedPropertyObject).OrderBy(menu => menu.DisplayName);
                            ContextMenu2.UpdateLayout();
                            ContextMenu2.IsOpen = true;
                            ContextMenu2.Dispatcher.BeginInvoke((Action)(() =>
                            {
                                ContextMenu2.Opacity = 1;
                                ContextMenu2.HorizontalOffset = -1 * (ContextMenu2.RenderSize.Width / dpiX) / 2;
                                ContextMenu2.VerticalOffset = -1 * (ContextMenu2.RenderSize.Height / dpiY) / 2;
                                ContextMenu2.Focus();
                            }),
                            System.Windows.Threading.DispatcherPriority.DataBind, null);
                        }
                        else
                        {
                            Popup.PreviewKeyDown += (_, ee) =>
                            {
                                if (ee.Key == Key.Escape)
                                {
                                    Popup.IsOpen = false;
                                }
                            };
                            Popup.Opacity = 0;
                            Popup.DataContext = resolvedPropertyObject;
                            Popup.UpdateLayout();
                            Popup.Child.UpdateLayout();
                            Popup.IsOpen = true;
                            Popup.Dispatcher.BeginInvoke((Action)(() =>
                            {
                                Popup.Opacity = 1;
                                Popup.HorizontalOffset = -1 * (Popup.Child.RenderSize.Width / dpiX) / 2;
                                Popup.VerticalOffset = -1 * (Popup.Child.RenderSize.Height / dpiY) / 2;
                                Keyboard.Focus(Popup.Child.FindVisualChild<Control>());
                            }),
                            System.Windows.Threading.DispatcherPriority.DataBind, null);
                        }
                    };
                    this.Inlines.Add(link);
                }
                this.Inlines.Add(new Run(" "));
            });
        }

        private void Popup_Loaded(object sender, RoutedEventArgs e)
        {
            Popup.UpdateLayout();
            Popup.Child.UpdateLayout();
            Popup.HorizontalOffset = -1 * Popup.Child.RenderSize.Width / 2;
            Popup.VerticalOffset = -1 * Popup.Child.RenderSize.Height / 2;
        }

        private void ContextMenu_Loaded(object sender, RoutedEventArgs e)
        {
            ContextMenu2.UpdateLayout();
            ContextMenu2.HorizontalOffset = -1 * ContextMenu2.RenderSize.Width / 2;
            ContextMenu2.VerticalOffset = -1 * ContextMenu2.RenderSize.Height / 2;
        }

        private List<ContextMenuItem> GetContextMenuFromOptionViewModel(IOptionViewModel options)
        {
            return options.All.Select(item => new ContextMenuItem
            {
                DisplayName = item.DisplayName,
                IsChecked = (item == options.Selected),
                Command = new RelayCommand(() => options.Selected = item),
            }).ToList();
        }

        private void ReadLinksAndText(string text, Action<string, bool> callback)
        {
            int ptr = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '{')
                {
                    if (i > 0)
                    {
                        callback(text.Substring(ptr, i - 1 - ptr), false);
                    }
                    ptr = i + 1;
                }
                else if (text[i] == '}')
                {
                    callback(text.Substring(ptr, i - ptr), true);
                    ptr = i + 1;
                }
            }

            if (ptr < text.Length - 1)
            {
                callback(text.Substring(ptr, text.Length - ptr), false);
            }
        }
    }
}
