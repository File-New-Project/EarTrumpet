using EarTrumpet.UI.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EarTrumpet.Interop.Helpers
{
    class StartupWindows
    {
        //public static void SetStartup(bool Defult=false)
        //{
        //    RegistryKey rk = Registry.CurrentUser.OpenSubKey
        //        ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        //    if (Defult)
        //        rk.SetValue(Application.ProductName, Application.ExecutablePath);
        //    else
        //        rk.DeleteValue(Application.ProductName, false);

        //}
        public static void SetStartup(bool start)
        {
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
            var r = rk.GetValue(Application.ProductName);
            return r != null;
        }
    }
}
