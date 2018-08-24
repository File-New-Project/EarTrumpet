using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;

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
    }
}
