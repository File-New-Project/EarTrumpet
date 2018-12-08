using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
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
                    var resolved = DataContext.GetType().GetProperty(prop).GetValue(DataContext, null).ToString();

                    this.Inlines.Add(new Hyperlink(new Run(resolved)));
                }
                this.Inlines.Add(new Run(" "));
            }
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
