using EarTrumpet.UI;
using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.ViewModels;
using EarTrumpet_Actions.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;

namespace EarTrumpet_Actions.Controls
{
    public class LinkedTextBlock : TextBlock
    {
        public string FormatText
        {
            get { return (string)this.GetValue(PeakValue1Property); }
            set { this.SetValue(PeakValue1Property, value); }
        }
        public static readonly DependencyProperty PeakValue1Property = DependencyProperty.Register(
          "FormatText", typeof(string), typeof(LinkedTextBlock), new PropertyMetadata("", new PropertyChangedCallback(FormatTextChanged)));

        private static void FormatTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((LinkedTextBlock)d).FormatTextChanged();

        private void FormatTextChanged()
        {
            this.Inlines.Clear();

            ReadLinksAndText(FormatText, (text, isLink) =>
            {
                if (!isLink)
                {
                    this.Inlines.Add(new Run(text));
                }
                else
                {
                    var resolvedPropertyObject = DataContext.GetType().GetProperty(text).GetValue(DataContext, null);
                    var link = new Hyperlink(new Run(resolvedPropertyObject.ToString()));
                    link.NavigateUri = new Uri("about:none");
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
