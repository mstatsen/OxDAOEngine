using OxXMLEngine.Data;
using OxXMLEngine.Settings;

namespace OxXMLEngine.Export
{
    public static class ExporterFactory
    {
        public static Exporter<TField, TDAO> Exporter<TField, TDAO>(ExportSettings<TField, TDAO> settings)
            where TField : notnull, Enum
            where TDAO : RootDAO<TField>, new() =>
            settings.Format switch
            {
                ExportFormat.Html => 
                    new HtmlExporter<TField, TDAO>(settings),
                ExportFormat.Xml => 
                    new XmlExporter<TField, TDAO>(settings),
                _ => 
                    new TextExporter<TField, TDAO>(settings),
            };
    }
}