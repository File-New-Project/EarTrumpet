using Microsoft.Win32;

namespace EarTrumpet.Extensions
{
    public static class Program
    {
        public static void SetStartup()
        {
            //MessageBox.Show("SetStartups");
            RegistryKey rk = Registry.CurrentUser.OpenSubKey
                ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            var ret = ReadSetting("StartUp", "1");
            if (ret == "1")
            {
                rk.SetValue(System.Windows.Forms.Application.ProductName, System.Windows.Forms.Application.ExecutablePath);
                WriteSetting("StartUp", "1");
            }
            else
            {
                rk.DeleteValue(System.Windows.Forms.Application.ProductName, false);
                WriteSetting("StartUp", "0");
            }
            //MessageBox.Show("SetStartupe");
        }
        public static void AddStartup()
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey
                ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            rk.SetValue(System.Windows.Forms.Application.ProductName, System.Windows.Forms.Application.ExecutablePath);
            WriteSetting("StartUp", "1");
        }
        public static void RemoveStartup()
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey
                ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            rk.DeleteValue(System.Windows.Forms.Application.ProductName, false);
            WriteSetting("StartUp", "0");
        }
        public static string ReadSetting(string key, string Defult)
        {
            string ret = null;
            try
            {
                //TrafficWatch.Properties.Settings.Default.Reload();
                RegistryKey rk = Registry.CurrentUser.OpenSubKey
                ("SOFTWARE\\TrafficWatch", true);
                ret = rk.GetValue(key, Defult).ToString();
                //var x= TrafficWatch.Properties.Settings.Default.Properties[key].DefaultValue;
                //ret = (string)TrafficWatch.Properties.Settings.Default.Properties[key].DefaultValue; ;
            }
            catch// (Exception ex)
            {
                RegistryKey rk = Registry.CurrentUser.CreateSubKey
                 ("SOFTWARE\\TrafficWatch");
                rk.SetValue(key, Defult);
                //TrafficWatch.Properties.Settings.Default.Properties.Add(new System.Configuration.SettingsProperty(key));
                return ret;
            }
            return ret;
        }
        public static void WriteSetting(string key, string value)
        {

            try
            {
                RegistryKey rk = Registry.CurrentUser.OpenSubKey
                   ("SOFTWARE\\TrafficWatch", true);
                rk.SetValue(key, value);
                //TrafficWatch.Properties.Settings.Default.Properties[key].DefaultValue = value;
                //Windows.Storage.ApplicationData.Current.LocalSettings.Values[key] = value;
            }
            catch //(Exception ex)
            {
                RegistryKey rk = Registry.CurrentUser.CreateSubKey
                  ("SOFTWARE\\TrafficWatch");
                rk.SetValue(key, value);
                // Windows Bug: Windows Storage APIs are still unreliable
                //TrafficWatch.Properties.Settings.Default.Properties.Add(new System.Configuration.SettingsProperty(key));
            }
            //TrafficWatch.Properties.Settings.Default.Save();
        }
    }
}
