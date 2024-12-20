﻿using OxDAOEngine.Data.Types;

namespace OxDAOEngine.ControlFactory
{
    public class ControlScopeHelper : AbstractTypeHelper<ControlScope>
    {
        public bool IsQuickFilter(ControlScope scope) =>
            scope is ControlScope.QuickFilter;

        public bool IsView(ControlScope scope) =>
            scope is ControlScope.CardView
                  or ControlScope.IconView
                  or ControlScope.InfoView;

        public bool IsSorting(ControlScope scope) =>
            scope is ControlScope.Sorting
                  or  ControlScope.Grouping;

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
                ControlScope.InfoView => "Full game info",
                ControlScope.CardView => "Game card",
                ControlScope.IconView => "Game icon",
                ControlScope.Category => "Categories",
                ControlScope.Export => "Export",
                ControlScope.Inline => "Inline",
                _ => "Unknown",
            };

        public bool SupportClickedLabels(ControlScope scope) =>
            scope is ControlScope.CardView
                  or ControlScope.Editor
                  or ControlScope.InfoView;
    }
}