using System;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;

namespace EarTrumpet.Extensibility.Hosting
{
    public class Addon
    {
        public string DisplayName { get; }

        private DirectoryCatalog _catalog;

        public Addon(DirectoryCatalog catalog)
        {
            _catalog = catalog;

            if (catalog.LoadedFiles.Count == 0)
            {
                DisplayName = string.Format(Properties.Resources.NoFilesLoadedFromAddonFormatText, catalog.Path);
            }
            else
            {
                DisplayName = string.Join(", \r\n", catalog.LoadedFiles.Select(f => Path.GetFileNameWithoutExtension(f))).TrimEnd(',').Trim();
            }
        }

        public bool IsAssembly(Assembly asm)
        {
            return _catalog.LoadedFiles.Any(file => file.ToLower() == asm.Location.ToLower());
        }
    }
}
