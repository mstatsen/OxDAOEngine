﻿using OxDAOEngine.ControlFactory;
using OxDAOEngine.Data.Types;
using OxDAOEngine.Settings.Part;

namespace OxDAOEngine.Data.Fields.Types
{
    public class FieldVariantHelper : AbstractStyledTypeHelper<FieldsVariant>
    {
        private readonly Dictionary<FieldsVariant, ControlScope> VariantScope =
            new()
            {
                [FieldsVariant.Table] = ControlScope.Table,
                [FieldsVariant.Html] = ControlScope.Html,
                [FieldsVariant.QuickFilter] = ControlScope.QuickFilter,
                [FieldsVariant.QuickFilterText] = ControlScope.QuickFilter,
                [FieldsVariant.Category] = ControlScope.Category,
                [FieldsVariant.Inline] = ControlScope.Inline,
                [FieldsVariant.BatchUpdate] = ControlScope.BatchUpdate
            };

        public override string GetName(FieldsVariant variant) =>
            variant switch
            {
                FieldsVariant.Table => "Table fields",
                FieldsVariant.QuickFilter => "Quick filter fields",
                FieldsVariant.QuickFilterText => "Text filter",
                FieldsVariant.Category => "Add categories, grouped by",
                FieldsVariant.Inline => "Inline fields",
                FieldsVariant.Html => "Fields",
                FieldsVariant.BatchUpdate => "Update batch",
                _ => "Fields",
            };

        public static bool Sortable(FieldsVariant variant) =>
            variant is not FieldsVariant.QuickFilter
                   and not FieldsVariant.QuickFilterText;

        public static FieldsVariant Variant(SettingsPart part) =>
            part switch
            {
                SettingsPart.Table => 
                    FieldsVariant.Table,
                SettingsPart.QuickFilter => 
                    FieldsVariant.QuickFilter,
                SettingsPart.QuickFilterText => 
                    FieldsVariant.QuickFilterText,
                SettingsPart.Category => 
                    FieldsVariant.Category,
                _ => FieldsVariant.Table,
            };

        public ControlScope Scope(FieldsVariant variant) =>
            VariantScope[variant];

        public FieldsVariant VariantByScope(ControlScope scope)
        {
            foreach (var item in VariantScope)
                if (item.Value.Equals(scope))
                    return item.Key;

            return FieldsVariant.Table;
        }

        public override FieldsVariant EmptyValue() =>
            FieldsVariant.Table;

        public override Color GetBaseColor(FieldsVariant value) =>
            value switch
            {
                FieldsVariant.QuickFilter => EngineStyles.QuickFilterColor,
                FieldsVariant.QuickFilterText => EngineStyles.QuickFilterTextColor,
                FieldsVariant.Category => EngineStyles.CategoryColor,
                FieldsVariant.Inline => EngineStyles.InlineColor,
                _ => EngineStyles.FieldsColor,
            };

        public override Color GetFontColor(FieldsVariant value) =>
            default!;
    }
}
