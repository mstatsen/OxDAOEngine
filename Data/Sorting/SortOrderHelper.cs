using OxXMLEngine.Data.Types;

namespace OxXMLEngine.Data.Sorting
{
    public class SortOrderHelper : AbstractTypeHelper<SortOrder>
    {
        public override SortOrder EmptyValue() =>
            SortOrder.Ascending;

        public override string GetName(SortOrder value) =>
            value switch
            {
                SortOrder.Ascending => "Ascending",
                SortOrder.Descending => "Descinding",
                _ => "None",
            };

        public override string GetShortName(SortOrder value) =>
            value switch
            {
                SortOrder.Ascending => "asc",
                SortOrder.Descending => "desc",
                _ => string.Empty,
            };
    }
}