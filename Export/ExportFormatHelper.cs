using OxDAOEngine.Data.Types;

namespace OxDAOEngine.Export
{
    public class ExportFormatHelper : AbstractTypeHelper<ExportFormat>
    {
        public override ExportFormat EmptyValue() =>
            ExportFormat.Html;

        public override string GetName(ExportFormat value) => 
            value switch
            {
                ExportFormat.Html => "HTML",
                ExportFormat.Xml => "XML",
                ExportFormat.Text => "Text",
                _ => string.Empty,
            };

        public string FileExt(ExportFormat value) => 
            value switch
            {
                ExportFormat.Html => ".html",
                ExportFormat.Xml => ".xml",
                ExportFormat.Text => ".txt",
                _ => string.Empty,
            };

        public string FileFilter(ExportFormat value)
        {
            string fileNameTemplate = $"*{FileExt(value)}";
            return $"{Name(value)} files ({fileNameTemplate}) | {fileNameTemplate};";
        }
    }
}