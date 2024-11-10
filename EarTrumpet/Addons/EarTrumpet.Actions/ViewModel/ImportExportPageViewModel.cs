using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.ViewModels;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Input;

namespace EarTrumpet.Actions.ViewModel;

public class ImportExportPageViewModel : SettingsPageViewModel
{
    public ICommand Import { get; }
    public ICommand Export { get; }

    private ActionsCategoryViewModel _parent;

    public ImportExportPageViewModel(ActionsCategoryViewModel parent) : base(DefaultManagementGroupName)
    {
        _parent = parent;
        Title = Properties.Resources.ImportAndExportTitle;
        Glyph = "\xE148";

        Import = new RelayCommand(OnImport);
        Export = new RelayCommand(OnExport);
    }

    private void OnImport()
    {
        var dlg = new Microsoft.Win32.OpenFileDialog
        {
            FileName = ".eta-xml",
            DefaultExt = ".eta-xml",
            Filter = $"{Properties.Resources.EtaXmlFileText}|*.eta-xml"
        };

        if (dlg.ShowDialog() == true)
        {
            try
            {
                EarTrumpetActionsAddon.Current.Import(dlg.FileName);
                _parent.ReloadSavedPages();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }
    }

    private void OnExport()
    {
        var dlg = new Microsoft.Win32.SaveFileDialog
        {
            FileName = ".eta-xml",
            DefaultExt = ".eta-xml",
            Filter = $"{Properties.Resources.EtaXmlFileText}|*.eta-xml"
        };

        if (dlg.ShowDialog() == true)
        {
            try
            {
                File.WriteAllText(dlg.FileName, EarTrumpetActionsAddon.Current.Export(), Encoding.Unicode);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }
    }
}
