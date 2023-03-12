using OxXMLEngine.Data;
using OxXMLEngine.Settings;
using System.Diagnostics;

namespace OxXMLEngine.Export
{
    public class ExportController<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        private static ExportSettings<TField, TDAO> Settings => 
            SettingsManager.DAOSettings<TField, TDAO>().ExportSettings;

        public ExportController() => 
            settingsForm = new ExportSettingsForm<TField, TDAO>(ExportController<TField, TDAO>.Settings);

        public void Export()
        {
            if (!PrepareToExport())
                return;

            SaveFile();
            OpenReadyFile();
        }

        private static void SaveFile()
        {
            try
            {
                StreamWriter sw = new(ExportController<TField, TDAO>.Settings.FileName);
                sw.Write(ExporterFactory.Exporter(ExportController<TField, TDAO>.Settings).Text());
                sw.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
        }

        private bool PrepareToExport() =>
            settingsForm.ShowDialog() == DialogResult.OK;

        private static void OpenReadyFile() =>
            Process.Start(ExportController<TField, TDAO>.Settings.FileName);

        private readonly ExportSettingsForm<TField, TDAO> settingsForm;
    }
}
