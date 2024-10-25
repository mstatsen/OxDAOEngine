﻿using OxDAOEngine.Data.Types;

namespace OxDAOEngine.Data.Sorting.Types
{
    public class SortingVariantHelper : AbstractStyledTypeHelper<SortingVariant>
    {
        public override SortingVariant EmptyValue() => SortingVariant.Global;

        public override Color GetBaseColor(SortingVariant value) =>
            value == SortingVariant.GroupBy ? EngineStyles.GroupByColor : EngineStyles.SortingColor;

        public override Color GetFontColor(SortingVariant value) => Color.Black;

        public override string GetName(SortingVariant value) =>
            value == SortingVariant.GroupBy ? "Group by" : "Sorting";

        public static string ClearButtonText(SortingVariant value) =>
            value == SortingVariant.GroupBy ? "Clear" : "Default";
    }
}