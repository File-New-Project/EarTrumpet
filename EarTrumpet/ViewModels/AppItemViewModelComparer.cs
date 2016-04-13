using System;
using System.Collections.Generic;

namespace EarTrumpet.ViewModels
{
    public class AppItemViewModelComparer : IComparer<AppItemViewModel>
    {
        public static AppItemViewModelComparer Instance = new AppItemViewModelComparer();

        public int Compare(AppItemViewModel one, AppItemViewModel two)
        {
            return string.Compare(one.ExeName, two.ExeName, StringComparison.Ordinal);
        }
    }
}
