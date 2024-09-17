using OxDAOEngine.Data.Types;

namespace OxDAOEngine.Settings.Export
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