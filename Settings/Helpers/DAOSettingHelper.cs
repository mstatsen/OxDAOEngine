using OxXMLEngine.Data.Extract;
using OxXMLEngine.Data.Filter;
using OxXMLEngine.View;

namespace OxXMLEngine.Settings.Helpers
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
                DAOSetting.AutoExpandCategories => "Auto expand categories",
                DAOSetting.ShowItemInfo => "Show info panel",
                DAOSetting.ShowIcons => "Show Icons view",
                DAOSetting.ShowCards => "Show Cards view",
                DAOSetting.SummarySorting => "Summary Sorting",
                DAOSetting.QuickFilterTextFieldOperation => "'Text' field filtering operation",
                DAOSetting.CategoryPanelPinned => "Category panel pinned",
                DAOSetting.CategoryPanelExpanded => "Category panel expanded",
                DAOSetting.ItemInfoPanelPinned => "Item info panel pinned",
                DAOSetting.ItemInfoPanelExpanded => "Item info panel expanded",
                DAOSetting.QuickFilterPinned => "Quick filter pinned",
                DAOSetting.QuickFilterExpanded => "Quick filter expanded",
                DAOSetting.CurrentView => "Current view",
                _ => string.Empty,
            };

        public override bool IsDAOSetting(DAOSetting setting) =>
            setting == DAOSetting.IconMapping;

        public override object? Default(DAOSetting setting) =>
            setting switch
            {
                DAOSetting.ShowCategories or
                DAOSetting.HideEmptyCategory or
                DAOSetting.ShowItemInfo or
                DAOSetting.ShowIcons or
                DAOSetting.ShowCards =>
                    true,
                DAOSetting.AutoExpandCategories or
                DAOSetting.CategoryPanelPinned or
                DAOSetting.CategoryPanelExpanded or
                DAOSetting.ItemInfoPanelExpanded or
                DAOSetting.ItemInfoPanelPinned or
                DAOSetting.QuickFilterPinned or
                DAOSetting.QuickFilterExpanded =>
                    false,
                DAOSetting.CardsPageSize =>
                    12,
                DAOSetting.IconsSize =>
                    IconSize.Medium,
                DAOSetting.IconsPageSize =>
                    30,
                DAOSetting.IconClickVariant =>
                    IconClickVariant.ShowCard,
                DAOSetting.SummarySorting =>
                    ExtractCompareType.Default,
                DAOSetting.QuickFilterTextFieldOperation =>
                    TextFilterOperation.Contains,
                DAOSetting.CurrentView => ItemsViewsType.Table,
                _ => null,
            };


        public override SettingsPart Part(DAOSetting setting) =>
            setting switch
            {
                DAOSetting.ShowCategories or
                DAOSetting.HideEmptyCategory or
                DAOSetting.AutoExpandCategories =>
                    SettingsPart.Category,
                DAOSetting.ShowItemInfo =>
                    SettingsPart.Table,
                DAOSetting.ShowCards or
                DAOSetting.CardsPageSize or
                DAOSetting.ShowIcons or
                DAOSetting.IconsSize or
                DAOSetting.IconsPageSize or
                DAOSetting.IconMapping or
                DAOSetting.IconClickVariant =>
                    SettingsPart.View,
                DAOSetting.QuickFilterTextFieldOperation =>
                    SettingsPart.QuickFilter,
                DAOSetting.SummarySorting =>
                    SettingsPart.Summary,
                _ => SettingsPart.Full,
            };

        protected override bool IsVisible(DAOSetting setting) =>
            setting switch
            {
                DAOSetting.ShowItemInfo or
                DAOSetting.ShowCategories or
                DAOSetting.HideEmptyCategory or
                DAOSetting.AutoExpandCategories or
                DAOSetting.ShowCards or
                DAOSetting.CardsPageSize or
                DAOSetting.ShowIcons or
                DAOSetting.IconsPageSize or
                DAOSetting.IconMapping or
                DAOSetting.IconsSize or
                DAOSetting.IconClickVariant or
                DAOSetting.QuickFilterTextFieldOperation =>
                    true,//case DAOSetting.SummarySorting:
                _ => false,
            };

        public override SettingList<DAOSetting> CardSettings =>
            new()
            {
                DAOSetting.ShowCards,
                DAOSetting.CardsPageSize
            };

        public override SettingList<DAOSetting> IconSettings =>
            new()
            {
                DAOSetting.ShowIcons,
                DAOSetting.IconsPageSize,
                DAOSetting.IconsSize,
                DAOSetting.IconMapping,
                DAOSetting.IconClickVariant
            };

        public override int ControlWidth(DAOSetting setting) =>
            setting switch
            {
                DAOSetting.IconClickVariant => 160,
                DAOSetting.SummarySorting => 110,
                DAOSetting.IconsSize => 80,
                DAOSetting.IconMapping => 200,
                DAOSetting.QuickFilterTextFieldOperation => 90,
                _ => base.ControlWidth(setting),
            };
    }
}