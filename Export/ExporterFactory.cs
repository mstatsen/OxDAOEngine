using OxDAOEngine.Data;
using OxDAOEngine.Settings;
using OxDAOEngine.Settings.Export;

namespace OxDAOEngine.Export
{
    public static class ExporterFactory
    {
        public static Exporter<TField, TDAO> Exporter<TField, TDAO>(ExportSettings<TField, TDAO> settings, 
            ExportSettingsForm<TField, TDAO> settingsForm)
            where TField : notnull, Enum
            where TDAO : RootDAO<TField>, new() =>
            settings.Format switch
            {
                ExportFormat.Html => 
                    new HtmlExporter<TField, TDAO>(settings, settingsForm),
                ExportFormat.Xml => 
                    new XmlExporter<TField, TDAO>(settings, settingsForm),
                _ => 
                    new TextExporter<TField, TDAO>(settings, settingsForm),
            };
    }
}