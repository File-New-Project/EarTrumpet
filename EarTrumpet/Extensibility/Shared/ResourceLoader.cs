using System;
using System.Collections.Generic;
using System.Windows;

namespace EarTrumpet.Extensibility.Shared
{
    public class ResourceLoader
    {
        static List<string> _namespaces = new List<string>();

        public static void Load(string addonNamespace)
        {
            if (!_namespaces.Contains(addonNamespace))
            {
                _namespaces.Add(addonNamespace);

                Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
                {
                    Source = new Uri($"/{addonNamespace};component/AddonResources.xaml", UriKind.RelativeOrAbsolute)
                });
            }
        }
    }
}
