using System;
using System.Collections.Generic;

namespace EarTrumpet.ViewModels
{
    public class DeviceViewModelComparer : IComparer<DeviceViewModel>
    {
        public static DeviceViewModelComparer Instance = new DeviceViewModelComparer();

        public int Compare(DeviceViewModel one, DeviceViewModel two)
        {
            return string.Compare(one.Id, two.Id, StringComparison.Ordinal);
        }
    }
}

