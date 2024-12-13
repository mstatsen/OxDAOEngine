using OxDAOEngine.Data;
using OxDAOEngine.Settings;
using OxDAOEngine.Settings.Export;
using System.Diagnostics;

namespace OxDAOEngine.Export
{
    public class ExportController<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        private static ExportSettings<TField, TDAO> Settings => 
            SettingsManager.DAOSettings<TField, TDAO>().ExportSettings;

        public ExportController() => 
            settingsForm = new ExportSettingsForm<TField, TDAO>(Settings);

        public void Export(RootListDAO<TField, TDAO>? selectedItems = null)
        {
            if (!PrepareToExport(selectedItems))
                return;

            SaveFile();
            OpenReadyFile();
        }

        private void SaveFile()
        {
            try
            {
                StreamWriter sw = new(Settings.FileName);
                sw.Write(ExporterFactory.Exporter(Settings, settingsForm).Text());
                sw.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception: {e.Message}");
            }
        }

        private bool PrepareToExport(RootListDAO<TField, TDAO>? selectedItems = null)
        {
            settingsForm.SelectedItems = selectedItems;
            return 
                settingsForm.ShowDialogIsOK(
                    DataManager.ListController<TField, TDAO>().Face
                );
        }

        private static void OpenReadyFile()
        {
            Process.Start(
                new ProcessStartInfo
                {
                    FileName = Settings.FileName,
                    UseShellExecute = true
                }
            );
        }

        private readonly ExportSettingsForm<TField, TDAO> settingsForm;
    }
}