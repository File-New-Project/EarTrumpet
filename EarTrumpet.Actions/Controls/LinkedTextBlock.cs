using EarTrumpet.UI;
using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.ViewModels;
using EarTrumpet_Actions.ViewModel;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;

namespace EarTrumpet_Actions.Controls
{
    public class LinkedTextBlock : TextBlock
    {
        class TText
        {
            public string Data;
        }

        class Link
        {
            public string Data;
        }

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

            var inlines = GetInlines(FormatText);

            foreach (var element in inlines)
            {
                if (element is TText)
                {
                    this.Inlines.Add(new Run(((TText)element).Data));
                }
                else
                {
                    var prop = ((Link)element).Data;
                    var propData = DataContext.GetType().GetProperty(prop).GetValue(DataContext, null);
                    var link = new Hyperlink(new Run(propData.ToString()));
                    link.NavigateUri = new Uri("about:none");
                    link.RequestNavigate += (s, e) =>
                    {

                        if (propData is IOptionViewModel)
                        {
                            var contextMenu = ThemedContextMenu.CreateThemedContextMenu(ThemeKind.LightOrDark, false);
                            contextMenu.ItemsSource = GetContextMenuFromOptionViewModel((IOptionViewModel)propData);
                            contextMenu.Placement = PlacementMode.Mouse;
                            contextMenu.IsOpen = true;
                        }
                        else
                        {
                            var p = (Popup)Application.Current.Resources["ActionsPopup"];
                            p.DataContext = propData;
                            p.Placement = PlacementMode.MousePoint;
                            p.StaysOpen = false;
                            p.IsOpen = true;
                        } 
                    };
                    this.Inlines.Add(link);
                }
                this.Inlines.Add(new Run(" "));
            }
        }

        private List<ContextMenuItem> GetContextMenuFromOptionViewModel(IOptionViewModel options)
        {
            var ret = new List<ContextMenuItem>();
            foreach(var item in options.All)
            {
                ret.Add(new ContextMenuItem
                {
                    DisplayName = item.DisplayName,
                    Command = new RelayCommand(() =>
                    {
                        options.Selected = item;
                    }),
                    IsChecked = (item == options.Selected),
                });
            }
            return ret;
        }

        private List<object> GetInlines(string text)
        {
            var ret = new List<object>();
            int ptr = 0;
            for (int i = 0; i < text.Length; i++)
            {
                switch (text[i])
                {
                    case '{':
                        if (i > 0)
                        {
                            ret.Add(new TText { Data = text.Substring(ptr, i - 1 - ptr) });
                        }
                        ptr = i + 1;
                        break;
                    case '}':
                        ret.Add(new Link { Data = text.Substring(ptr, i - ptr) });
                        ptr = i + 1;
                        break;
                    default:
                        break;
                }
            }

            if (ptr < text.Length - 1)
            {
                ret.Add(new TText { Data = text.Substring(ptr, text.Length - ptr) });
            }

            return ret;
        }
    }
}
