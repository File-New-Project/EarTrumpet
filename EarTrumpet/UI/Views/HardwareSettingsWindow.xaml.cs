using System.Windows;
using System.Windows.Forms;

namespace EarTrumpet.UI.Views
{
    public partial class HardwareSettingsWindow : Window
    {
        public HardwareSettingsWindow()
        {
            InitializeComponent();
        }

        private void ButtonSaveMidiControlCommand_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("ToDo", "Caption", MessageBoxButtons.OK);
        }

        private void ButtonSelectMidiDeviceControl_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("ToDo", "Caption", MessageBoxButtons.OK);
        }

    }
}