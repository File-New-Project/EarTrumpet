using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EarTrumpet.UI.Services
{
    class StartupWindows
    {
        public static void SetStartup()
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey
                ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            var ret = SettingsService.S_settings.Get("StartUp", false);
            if (ret)
                rk.SetValue(Application.ProductName, Application.ExecutablePath);
            else
                rk.DeleteValue(Application.ProductName, false);

        }
        public static void SetStartup(bool start)
        {
            SettingsService.S_settings.Set("StartUp", start);
            RegistryKey rk = Registry.CurrentUser.OpenSubKey
                ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (start)
            {
                rk.SetValue(Application.ProductName, Application.ExecutablePath);
            }
            else
            {
                rk.DeleteValue(Application.ProductName, false);
            }
        }
        public static bool GetStartup()
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey
                ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            var r=rk.GetValue(Application.ProductName);
            if(r==null)
                SettingsService.S_settings.Set("StartUp",false);
            else
                SettingsService.S_settings.Set("StartUp", true);
            return r!=null;
        }
    }
}
