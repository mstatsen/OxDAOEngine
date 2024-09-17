using OxDAOEngine.Data.Types;
using System.Drawing;

namespace OxDAOEngine.Data.Sorting
{
    public class SortingVariantHelper : AbstractStyledTypeHelper<SortingVariant>
    {
        public override SortingVariant EmptyValue() => SortingVariant.Global;

        public override Color GetBaseColor(SortingVariant value) =>
            value == SortingVariant.GroupBy ? EngineStyles.GroupByColor : EngineStyles.SortingColor;

        public override Color GetFontColor(SortingVariant value) => Color.Black;

        public override string GetName(SortingVariant value) =>
            value == SortingVariant.GroupBy ? "Group by" : "Sorting";

        public string ClearButtonText(SortingVariant value) =>
            value == SortingVariant.GroupBy ? "Clear" : "Default";
    }
}
