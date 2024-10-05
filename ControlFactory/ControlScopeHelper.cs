﻿using OxDAOEngine.Data.Types;

namespace OxDAOEngine.ControlFactory
{
    public class ControlScopeHelper : AbstractTypeHelper<ControlScope>
    {
        public static bool IsQuickFilter(ControlScope scope) =>
            scope == ControlScope.QuickFilter;

        public static bool IsView(ControlScope scope) =>
            scope is ControlScope.CardView
            or ControlScope.IconView
            or ControlScope.FullInfoView;

        public static bool IsSorting(ControlScope scope) =>
            scope == ControlScope.Sorting
            || scope == ControlScope.Grouping;

        public override ControlScope EmptyValue() => 
            ControlScope.Editor;

        public override string GetName(ControlScope value) => 
            value switch
            {
                ControlScope.Editor => "Editor",
                ControlScope.QuickFilter => "Quick filter",
                ControlScope.BatchUpdate => "Batch update",
                ControlScope.Sorting => "Sorting",
                ControlScope.Grouping => "Group by",
                ControlScope.Table => "Table",
                ControlScope.Html => "HTML",
                ControlScope.FullInfoView => "Full game info",
                ControlScope.CardView => "Game card",
                ControlScope.IconView => "Game icon",
                ControlScope.Category => "Categories",
                ControlScope.Summary => "Summary",
                ControlScope.Export => "Export",
                ControlScope.Inline => "Inline",
                _ => "Unknown",
            };

        public static bool SupportClickedLabels(ControlScope scope) =>
            scope == ControlScope.CardView ||
            scope == ControlScope.Editor ||
            scope == ControlScope.FullInfoView ||
            scope == ControlScope.Summary;
    }
}