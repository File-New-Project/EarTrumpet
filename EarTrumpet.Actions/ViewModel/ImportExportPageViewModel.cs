using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.ViewModels;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;

namespace EarTrumpet_Actions.ViewModel
{
    public class ImportExportPageViewModel : SettingsPageViewModel
    {
        public ICommand Import { get; }
        public ICommand Export { get; }

        ActionsCategoryViewModel _parent;

        public ImportExportPageViewModel(ActionsCategoryViewModel parent) : base(DefaultManagementGroupName)
        {
            _parent = parent;
            Title = Properties.Resources.ImportAndExportTitle;
            Glyph = "\xE148";

            Import = new RelayCommand(OnImport);
            Export = new RelayCommand(OnExport);
        }

        void OnImport()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = ".eta-xml";
            dlg.DefaultExt = ".eta-xml";
            dlg.Filter = $"{Properties.Resources.EtaXmlFileText}|*.eta-xml";

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    Addon.Current.Import(dlg.FileName);
                    _parent.ReloadSavedPages();
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                }
            }
        }

        void OnExport()
        {
            var dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = ".eta-xml";
            dlg.DefaultExt = ".eta-xml";
            dlg.Filter = $"{Properties.Resources.EtaXmlFileText}|*.eta-xml";

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    File.WriteAllText(dlg.FileName, Addon.Current.Export());
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                }
            }
        }
    }
}
