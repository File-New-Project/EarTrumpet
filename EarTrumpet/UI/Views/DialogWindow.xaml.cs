using System.Diagnostics;
using System.Windows;

namespace EarTrumpet.UI.Views
{
    public partial class DialogWindow : Window
    {
        public DialogWindow()
        {
            Trace.WriteLine("DialogWindow .ctor");
            Closed += (_, __) => Trace.WriteLine("DialogWindow Closed");

            InitializeComponent();
        }
    }
}
