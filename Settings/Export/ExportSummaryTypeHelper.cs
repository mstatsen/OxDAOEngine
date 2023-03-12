using OxXMLEngine.Data.Types;

namespace OxXMLEngine.Settings
{
    public class ExportSummaryTypeHelper : AbstractTypeHelper<ExportSummaryType>
    {
        public override ExportSummaryType EmptyValue() => ExportSummaryType.Exported;

        public override string GetName(ExportSummaryType value) => 
            value switch
            {
                ExportSummaryType.None => "None",
                ExportSummaryType.Exported => "Exported",
                ExportSummaryType.Full => "Exported / Total",
                _ => string.Empty,
            };
    }
}