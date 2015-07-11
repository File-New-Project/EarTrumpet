using System.Windows;

namespace EarTrumpet
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            new MainWindow();
        }
    }
}
