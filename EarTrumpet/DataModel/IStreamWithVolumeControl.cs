using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EarTrumpet.DataModel
{
    public interface IStreamWithVolumeControl : INotifyPropertyChanged
    {
        string DisplayName { get; }
        string Id { get; }
        bool IsMuted { get; set; }
        float Volume { get; set; }
        float PeakValue { get; }
    }
}
