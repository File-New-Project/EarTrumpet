using EarTrumpet.UI.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace EarTrumpet.Extensions
{
    public class Language
    {
        #region Member variables
        public static App Instance;
        public static String Directory;
        public event EventHandler LanguageChangedEvent;
        #endregion

        #region Constructor
        public Language()
        {
            // Initialize static variables
            Directory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            // Load the Localization Resource Dictionary based on OS language
            SetLanguageResourceDictionary(GetLocXAMLFilePath(CultureInfo.CurrentCulture.Name));
            //Set Language
            string l = SettingsService.Language;
            if (l != "Auto")
                SwitchLanguage(l);
            //
        }

        #endregion
        #region Functions
        /// <summary>
        /// Dynamically load a Localization ResourceDictionary from a file
        /// </summary>
        public void SwitchLanguage(string inFiveCharLang)
        {
            CultureInfo ci;
            if (CultureInfo.CurrentCulture.Name.Equals(inFiveCharLang) && CultureInfo.CurrentUICulture.Name.Equals(inFiveCharLang))
                return;
            if (inFiveCharLang == "Auto")
                ci = CultureInfo.InstalledUICulture;
            else
                ci = new CultureInfo(inFiveCharLang);
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;

            SetLanguageResourceDictionary(GetLocXAMLFilePath(inFiveCharLang));
            LanguageChangedEvent?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Returns the path to the ResourceDictionary file based on the language character string.
        /// </summary>
        /// <param name="inFiveCharLang"></param>
        /// <returns></returns>
        private string GetLocXAMLFilePath(string inFiveCharLang)
        {
            string locXamlFile = "LocalizationDictionary." + inFiveCharLang + ".xaml";
            return Path.Combine(Directory, inFiveCharLang, locXamlFile);
        }

        /// <summary>
        /// Sets or replaces the ResourceDictionary by dynamically loading
        /// a Localization ResourceDictionary from the file path passed in.
        /// </summary>
        /// <param name="inFile"></param>
        private void SetLanguageResourceDictionary(string inFile)
        {
            if (File.Exists(inFile))
            {
                // Read in ResourceDictionary File
                var languageDictionary = new ResourceDictionary
                {
                    Source = new Uri(inFile)
                };

                // Remove any previous Localization dictionaries loaded
                int langDictId = -1;
                for (int i = 0; i < App.Current.Resources.MergedDictionaries.Count; i++)
                {
                    var md = App.Current.Resources.MergedDictionaries[i];
                    // Make sure your Localization ResourceDictionarys have the ResourceDictionaryName
                    // key and that it is set to a value starting with "Loc-".
                    if (md.Contains("ResourceDictionaryName"))
                    {
                        if (md["ResourceDictionaryName"].ToString().StartsWith("Loc-"))
                        {
                            langDictId = i;
                            break;
                        }
                    }
                }
                if (langDictId == -1)
                {
                    // Add in newly loaded Resource Dictionary
                    App.Current.Resources.MergedDictionaries.Add(languageDictionary);
                }
                else
                {
                    // Replace the current langage dictionary with the new one
                    App.Current.Resources.MergedDictionaries[langDictId] = languageDictionary;
                }
            }
        }
        #endregion

    }
}
