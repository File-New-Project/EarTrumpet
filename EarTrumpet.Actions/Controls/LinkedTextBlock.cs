using EarTrumpet.UI;
using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.ViewModels;
using EarTrumpet_Actions.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;

namespace EarTrumpet_Actions.Controls
{
    public class LinkedTextBlock : TextBlock
    {
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
                        if (resolvedPropertyObject is IOptionViewModel)
                        {
                            var contextMenu = ThemedContextMenu.CreateThemedContextMenu(ThemeKind.LightOrDark, false);
                            contextMenu.ItemsSource = GetContextMenuFromOptionViewModel((IOptionViewModel)resolvedPropertyObject);
                            contextMenu.Placement = PlacementMode.Mouse;
                            contextMenu.IsOpen = true;
                        }
                        else if (resolvedPropertyObject is HotkeyViewModel)
                        {
                            var windowVm = (IWindowHostedViewModelInternal)Window.GetWindow(this).DataContext;
                            var vm = new HotkeySelectViewModel();
                            windowVm.HostDialog(vm);
                            if (vm.Saved)
                            {
                                ((HotkeyViewModel)resolvedPropertyObject).Hotkey = vm.Hotkey;
                            }
                        }
                        else
                        {
                            var p = (Popup)Application.Current.Resources["ActionsPopup"];
                            p.DataContext = resolvedPropertyObject;
                            p.Placement = PlacementMode.Mouse;
                            p.IsOpen = true;
                        }
                    };
                    this.Inlines.Add(link);
                }
                this.Inlines.Add(new Run(" "));
            });
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
