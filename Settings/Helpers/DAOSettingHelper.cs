using OxDAOEngine.ControlFactory;
using OxDAOEngine.Data.Fields;
using OxDAOEngine.Data.Filter.Types;
using OxDAOEngine.Data.Types;
using OxDAOEngine.Settings.Part;
using OxDAOEngine.View.Types;

namespace OxDAOEngine.Settings.Helpers
{
    public class DAOSettingHelper : SettingHelper<DAOSetting>
    {
        public override string GetName(DAOSetting setting) =>
            setting switch
            {
                DAOSetting.CardsPageSize => "Cards' View page size",
                DAOSetting.IconsPageSize => "Icons' View page size",
                DAOSetting.IconsSize => "Icons' size",
                DAOSetting.IconMapping => "Icon mapping",
                DAOSetting.IconClickVariant => "When click on icon",
                DAOSetting.ShowCategories => "Show Categories",
                DAOSetting.HideEmptyCategory => "Hide empty categories",
                DAOSetting.ShowItemInfo => "Info panel",
                DAOSetting.ShowIcons => "Show Icons view",
                DAOSetting.ShowCards => "Show Cards view",
                DAOSetting.ShowQuickFilter => "Quick Filter",
                DAOSetting.QuickFilterTextFieldOperation => "'Text' field filtering operation",
                DAOSetting.CategoryPanelPinned => "Category panel pinned",
                DAOSetting.CategoryPanelExpanded => "Category panel expanded",
                DAOSetting.ItemInfoPanelPinned => "Item info panel pinned",
                DAOSetting.ItemInfoPanelExpanded => "Item info panel expanded",
                DAOSetting.ItemInfoPosition => "Info panel position",
                DAOSetting.QuickFilterPinned => "Quick filter pinned",
                DAOSetting.QuickFilterExpanded => "Quick filter expanded",
                DAOSetting.CurrentView => "Current view",
                DAOSetting.AlwaysExpandedCategories => "Always expanded",
                _ => string.Empty,
            };

        public override bool IsDAOSetting(DAOSetting setting) =>
            setting is DAOSetting.IconMapping;

        public override object? Default(DAOSetting setting) =>
            setting switch
            {
                DAOSetting.HideEmptyCategory or
                DAOSetting.ShowIcons or
                DAOSetting.ShowCards =>
                    true,
                DAOSetting.CategoryPanelPinned or
                DAOSetting.CategoryPanelExpanded or
                DAOSetting.ItemInfoPanelExpanded or
                DAOSetting.ItemInfoPanelPinned or
                DAOSetting.QuickFilterPinned or
                DAOSetting.QuickFilterExpanded or
                DAOSetting.AlwaysExpandedCategories =>
                    false,
                DAOSetting.ItemInfoPosition =>
                    ItemInfoPosition.Right,
                DAOSetting.CardsPageSize =>
                    12,
                DAOSetting.IconsSize =>
                    IconSize.Medium,
                DAOSetting.IconsPageSize =>
                    30,
                DAOSetting.IconClickVariant =>
                    IconClickVariant.ShowCard,
                DAOSetting.QuickFilterTextFieldOperation =>
                    TextFilterOperation.Contains,
                DAOSetting.CurrentView => 
                    ItemsViewsType.Table,
                DAOSetting.ShowCategories or
                DAOSetting.ShowItemInfo or 
                DAOSetting.ShowQuickFilter =>
                    FunctionalPanelVisible.Float,
                _ => null,
            };


        public override SettingsPart Part(DAOSetting setting) =>
            setting switch
            {
                DAOSetting.ShowCategories or
                DAOSetting.AlwaysExpandedCategories or
                DAOSetting.HideEmptyCategory =>
                    SettingsPart.Category,
                DAOSetting.ShowItemInfo or 
                DAOSetting.ItemInfoPosition =>
                    SettingsPart.Table,
                DAOSetting.ShowCards or
                DAOSetting.CardsPageSize =>
                    SettingsPart.Cards,
                DAOSetting.ShowIcons or
                DAOSetting.IconsSize or
                DAOSetting.IconsPageSize or
                DAOSetting.IconMapping or
                DAOSetting.IconClickVariant =>
                    SettingsPart.Icons,
                DAOSetting.ShowQuickFilter or
                DAOSetting.QuickFilterTextFieldOperation =>
                    SettingsPart.QuickFilter,
                _ => SettingsPart.Full,
            };

        protected override bool IsVisible(DAOSetting setting) =>
            setting switch
            {
                DAOSetting.ShowItemInfo or
                DAOSetting.ItemInfoPosition or
                DAOSetting.ShowCategories or
                DAOSetting.AlwaysExpandedCategories or
                DAOSetting.HideEmptyCategory or
                DAOSetting.ShowCards or
                DAOSetting.CardsPageSize or
                DAOSetting.ShowIcons or
                DAOSetting.IconsPageSize or
                DAOSetting.IconMapping or
                DAOSetting.IconsSize or
                DAOSetting.IconClickVariant or
                DAOSetting.ShowQuickFilter or
                DAOSetting.QuickFilterTextFieldOperation =>
                    true,//case DAOSetting.SummarySorting:
                _ => false,
            };

        public override int ControlWidth(DAOSetting setting) =>
            setting switch
            {
                DAOSetting.IconClickVariant => 
                    160,
                DAOSetting.IconsSize =>
                    94,
                DAOSetting.ItemInfoPosition =>
                    80,
                DAOSetting.IconMapping => 
                    200,
                DAOSetting.ShowCategories or
                DAOSetting.ShowItemInfo or
                DAOSetting.ShowQuickFilter =>
                    148,
                DAOSetting.QuickFilterTextFieldOperation => 
                    90,
                _ => 
                    base.ControlWidth(setting),
            };

        public override bool WithoutLabel(DAOSetting setting) =>
            setting is
                DAOSetting.ShowCategories or
                DAOSetting.ShowQuickFilter;

        public override FieldType GetFieldType(DAOSetting field) =>
            field switch
            {
                DAOSetting.IconMapping =>
                    FieldType.List,
                DAOSetting.CategoryPanelPinned or
                DAOSetting.CategoryPanelExpanded or
                DAOSetting.AlwaysExpandedCategories or
                DAOSetting.ItemInfoPanelPinned or
                DAOSetting.ItemInfoPanelExpanded or
                DAOSetting.QuickFilterPinned or
                DAOSetting.QuickFilterExpanded or
                DAOSetting.HideEmptyCategory or
                DAOSetting.ShowIcons or
                DAOSetting.ShowCards =>
                    FieldType.Boolean,
                DAOSetting.CardsPageSize or
                DAOSetting.IconsPageSize =>
                    FieldType.Integer,
                DAOSetting.CurrentView or
                DAOSetting.ShowItemInfo or
                DAOSetting.ItemInfoPosition or
                DAOSetting.IconsSize or
                DAOSetting.ShowCategories or
                DAOSetting.ShowQuickFilter or
                DAOSetting.IconClickVariant or
                DAOSetting.QuickFilterTextFieldOperation =>
                    FieldType.Enum,
                _ => FieldType.String,
            };

        public override ITypeHelper? GetHelper(DAOSetting field) =>
            field switch
            {
                DAOSetting.IconsSize =>
                    TypeHelper.Helper<IconSizeHelper>(),
                DAOSetting.ShowQuickFilter or
                DAOSetting.ShowItemInfo or
                DAOSetting.ShowCategories =>
                    TypeHelper.Helper<FunctionalPanelVisibleHelper>(),
                DAOSetting.IconClickVariant =>
                    TypeHelper.Helper<IconClickVariantHelper>(),
                DAOSetting.QuickFilterTextFieldOperation => 
                    TypeHelper.Helper<FilterOperationHelper>(),
                DAOSetting.ItemInfoPosition =>
                    TypeHelper.Helper<ItemInfoPositionHelper>(),
                _ => null,
            };
    }
}