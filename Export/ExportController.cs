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

        public void Export()
        {
            if (!PrepareToExport())
                return;

            SaveFile();
            OpenReadyFile();
        }

        private void SaveFile()
        {
            try
            {
                StreamWriter sw = new(ExportController<TField, TDAO>.Settings.FileName);
                sw.Write(ExporterFactory.Exporter(ExportController<TField, TDAO>.Settings, settingsForm).Text());
                sw.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
        }

        private bool PrepareToExport() =>
            settingsForm.ShowDialog() == DialogResult.OK;

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
