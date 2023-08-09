using OxXMLEngine.Data;
using OxXMLEngine.Data.Sorting;
using OxXMLEngine.Settings;
using OxXMLEngine.XML;
using System.Xml;

namespace OxXMLEngine.Export
{
    internal class XmlExporter<TField, TDAO> : Exporter<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public XmlExporter(ExportSettings<TField, TDAO> settings, ExportSettingsForm<TField, TDAO> settingsForm) 
            : base(settings, settingsForm)
        { }

        protected override FieldSortings<TField, TDAO>? Sortings() => ListController.DefaultSorting();

        public override string Text()
        {
            XmlDocument document = new();
            document.AppendChild(document.CreateElement("Data"));

            bool modified = Items.Modified;
            try
            {
                Items.Save(document.DocumentElement);
            }
            finally
            {
                Items.Modified = modified;
            }

            StringWriter stringWriter = new();
            XmlWriterSettings writerSettings = new()
            {
                Indent = Settings.XML.Indent,
                IndentChars = XmlConsts.DefaultIndent,
                NewLineChars = Environment.NewLine
            };

            XmlWriter xmlWriter = XmlWriter.Create(stringWriter, writerSettings);
            document.Save(xmlWriter);
            xmlWriter.Flush();

            return stringWriter.ToString();
        }
    }
}