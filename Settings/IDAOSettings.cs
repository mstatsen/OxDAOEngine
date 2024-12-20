﻿using OxDAOEngine.ControlFactory;
using OxDAOEngine.ControlFactory.Controls.Fields;
using OxDAOEngine.ControlFactory.Filter;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Fields;
using OxDAOEngine.Data.Filter.Types;
using OxDAOEngine.Settings.Data;
using OxDAOEngine.Settings.Part;
using OxDAOEngine.View.Types;

namespace OxDAOEngine.Settings
{
    public interface IDAOSettings : ISettingsController<DAOSetting>
    {
        int CardsPageSize { get; set; }
        int IconsPageSize { get; set; }
        bool CategoryPanelPinned { get; set; }
        bool CategoryPanelExpanded { get; set; }
        bool AlwaysExpandedCategories { get; set; }
        bool HideEmptyCategory { get; set; }
        FunctionalPanelVisible ShowCategories { get; set; }
        FunctionalPanelVisible ShowItemInfo { get; set; }
        bool ShowIcons { get; set; }
        bool ShowCards { get; set; }
        bool CardsAllowDelete { get; set; }
        bool CardsAllowEdit { get; set; }
        bool CardsAllowExpand { get; set; }
        TextFilterOperation QuickFilterTextFieldOperation { get; set; }
        bool ItemInfoPanelPinned { get; set; }
        bool ItemInfoPanelExpanded { get; set; }
        bool QuickFilterPinned { get; set; }
        bool QuickFilterExpanded { get; set; }
        IconClickVariant IconClickVariant { get; set; }
        IconSize IconsSize { get; set; }
        //ExportSettings ExportSettings { get; }
        List<object> GetFields(SettingsPart part);
        void SetFields(SettingsPart part, List<object> value);
        IFieldsPanel CreateFieldsPanel(SettingsPart part, Control parent);
        ICategoriesPanel CreateCategoriesPanel(Control parent);
        bool AvailableSummary { get; set; }
        bool AvailableCategories { get; set; }
        bool AvailableQuickFilter { get; set; }
        bool AvailableCards { get; set; }
        bool AvailableIcons { get; set; }
        ItemsViewsType CurrentView { get; set; }
        IListDAO Categories { get; set; }
    }

    public interface IDAOSettings<TField> : IDAOSettings
        where TField : notnull, Enum
    {
        Dictionary<SettingsPart, FieldColumns<TField>> Fields { get; }
        FieldColumns<TField> TableFields { get; set; }
        FieldColumns<TField> QuickFilterFields { get; set; }
        FieldColumns<TField> QuickFilterTextFields { get; set; }
        ListDAO<IconMapping<TField>>? IconMapping { get; set; }
    }
}
