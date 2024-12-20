﻿using System.Xml;
using OxLibrary;
using OxDAOEngine.ControlFactory;
using OxDAOEngine.ControlFactory.Controls.Fields;
using OxDAOEngine.ControlFactory.Filter;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Fields;
using OxDAOEngine.Data.Fields.Types;
using OxDAOEngine.Data.Filter;
using OxDAOEngine.Data.Filter.Types;
using OxDAOEngine.Data.Sorting;
using OxDAOEngine.Data.Types;
using OxDAOEngine.SystemEngine;
using OxDAOEngine.Settings.ControlFactory;
using OxDAOEngine.Settings.Data;
using OxDAOEngine.Settings.Export;
using OxDAOEngine.Settings.Observers;
using OxDAOEngine.Settings.Part;
using OxDAOEngine.View.Types;
using OxDAOEngine.XML;

namespace OxDAOEngine.Settings
{
    public class DAOSettings<TField, TDAO> : SettingsController<DAOSetting>, IDAOSettings<TField>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        private readonly FieldSortings<TField, TDAO> sortings = new();
        public Dictionary<SettingsPart, FieldColumns<TField>> Fields => fields;

        public DAOSettings() : base() { }

        public int CardsPageSize
        {
            get => IntValue(DAOSetting.CardsPageSize);
            set => this[DAOSetting.CardsPageSize] = value;
        }

        public int IconsPageSize
        {
            get => IntValue(DAOSetting.IconsPageSize);
            set => this[DAOSetting.IconsPageSize] = value;
        }

        public bool CategoryPanelExpanded
        {
            get => BoolValue(DAOSetting.CategoryPanelExpanded);
            set => this[DAOSetting.CategoryPanelExpanded] = value;
        }

        public bool CategoryPanelPinned
        {
            get => BoolValue(DAOSetting.CategoryPanelPinned);
            set => this[DAOSetting.CategoryPanelPinned] = value;
        }

        public ItemsViewsType CurrentView
        {
            get => Parse<ItemsViewsType>(DAOSetting.CurrentView);
            set => this[DAOSetting.CurrentView] = value;
        }

        public bool HideEmptyCategory
        {
            get => BoolValue(DAOSetting.HideEmptyCategory);
            set => this[DAOSetting.HideEmptyCategory] = value;
        }

        public bool AlwaysExpandedCategories
        {
            get => BoolValue(DAOSetting.AlwaysExpandedCategories);
            set => this[DAOSetting.AlwaysExpandedCategories] = value;
        }

        public FunctionalPanelVisible ShowCategories
        {
            get => AvailableCategories 
                ? Parse<FunctionalPanelVisible>(DAOSetting.ShowCategories) 
                : FunctionalPanelVisible.Hidden;

            set => this[DAOSetting.ShowCategories] = AvailableCategories
                ? value 
                : (object) FunctionalPanelVisible.Hidden;
        }

        public FunctionalPanelVisible ShowQuickFilter
        {
            get => AvailableQuickFilter 
                ? Parse<FunctionalPanelVisible>(DAOSetting.ShowQuickFilter)
                : FunctionalPanelVisible.Hidden;

            set => this[DAOSetting.ShowQuickFilter] = AvailableQuickFilter
                ? value 
                : (object)FunctionalPanelVisible.Hidden;
        }

        public FunctionalPanelVisible ShowItemInfo
        {
            get => Parse<FunctionalPanelVisible>(DAOSetting.ShowItemInfo);
            set => this[DAOSetting.ShowItemInfo] = value;
        }

        public bool ShowIcons
        {
            get => BoolValue(DAOSetting.ShowIcons);
            set => this[DAOSetting.ShowIcons] = value;
        }

        public bool ShowCards
        {
            get => BoolValue(DAOSetting.ShowCards);
            set => this[DAOSetting.ShowCards] = value;
        }

        public bool CardsAllowDelete
        {
            get => BoolValue(DAOSetting.CardsAllowDelete);
            set => this[DAOSetting.CardsAllowDelete] = value;
        }

        public bool CardsAllowEdit
        {
            get => BoolValue(DAOSetting.CardsAllowEdit);
            set => this[DAOSetting.CardsAllowEdit] = value;
        }

        public bool CardsAllowExpand
        {
            get => BoolValue(DAOSetting.CardsAllowExpand);
            set => this[DAOSetting.CardsAllowExpand] = value;
        }

        private T? Parse<T>(DAOSetting setting)
            where T : Enum =>
            TypeHelper.Parse<T>(this[setting]!.ToString()!);

        public TextFilterOperation QuickFilterTextFieldOperation
        {
            get => Parse<TextFilterOperation>(DAOSetting.QuickFilterTextFieldOperation);
            set => this[DAOSetting.QuickFilterTextFieldOperation] = value;
        }

        public bool ItemInfoPanelPinned
        {
            get => BoolValue(DAOSetting.ItemInfoPanelPinned);
            set => this[DAOSetting.ItemInfoPanelPinned] = value;
        }

        public bool ItemInfoPanelExpanded
        {
            get => BoolValue(DAOSetting.ItemInfoPanelExpanded);
            set => this[DAOSetting.ItemInfoPanelExpanded] = value;
        }

        public ItemInfoPosition ItemInfoPosition
        {
            get => Parse<ItemInfoPosition>(DAOSetting.ItemInfoPosition);
            set => this[DAOSetting.ItemInfoPosition] = value;
        }

        public bool QuickFilterPinned
        {
            get => BoolValue(DAOSetting.QuickFilterPinned);
            set => this[DAOSetting.QuickFilterPinned] = value;
        }

        public bool QuickFilterExpanded
        {
            get => BoolValue(DAOSetting.QuickFilterExpanded);
            set => this[DAOSetting.QuickFilterExpanded] = value;
        }

        public IconClickVariant IconClickVariant
        {
            get => Parse<IconClickVariant>(DAOSetting.IconClickVariant);
            set => this[DAOSetting.IconClickVariant] = value;
        }

        public IconSize IconsSize
        {
            get => Parse<IconSize>(DAOSetting.IconsSize);
            set => this[DAOSetting.IconsSize] = value;
        }

        public Filter<TField, TDAO>? Filter
        {
            get => filter;
            set => filter.CopyFrom(value);
        }

        public ListDAO<IconMapping<TField>>? IconMapping
        {
            get => DAOValue<ListDAO<IconMapping<TField>>>(DAOSetting.IconMapping);
            set => this[DAOSetting.IconMapping] = value;
        }

        public FieldSortings<TField, TDAO> Sortings
        {
            get => sortings;
            set
            {
                sortings.StartSilentChange();

                try
                {
                    sortings.SortingsList = value.SortingsList;
                }
                finally
                {
                    sortings.FinishSilentChange();
                    Modified |= sortings.Modified;
                }
            }
        }

        public FieldColumns<TField> TableFields
        {
            get
            {
                FieldColumns<TField> result = fields[SettingsPart.Table];

                if (result.IsEmpty)
                {
                    TableFields = TypeHelper.FieldHelper<TField>()
                        .Columns(FieldsVariant.Table, FieldsFilling.Default);
                    result = fields[SettingsPart.Table];
                }

                return result;
            }
            set => SetFields(SettingsPart.Table, value);
        }

        public FieldColumns<TField> QuickFilterFields
        {
            get => GetFields(SettingsPart.QuickFilter);
            set => SetFields(SettingsPart.QuickFilter, value);
        }

        public FieldColumns<TField> QuickFilterTextFields
        {
            get
            {
                FieldColumns<TField> result = fields[SettingsPart.QuickFilterText];

                if (result.IsEmpty)
                {
                    QuickFilterTextFields = TypeHelper.FieldHelper<TField>()
                        .Columns(FieldsVariant.QuickFilterText, FieldsFilling.Default);
                    result = fields[SettingsPart.QuickFilterText];
                }

                return result;
            }
            set => SetFields(SettingsPart.QuickFilterText, value);
        }

        private readonly Categories<TField, TDAO> categories = new();
        public IListDAO Categories
        {
            get => categories;
            set => categories.CopyFrom(value);
        }

        public ExportSettings<TField, TDAO> ExportSettings => exportSettings;

        public override string ListName => DataManager.FieldController<TField>().ListName;
        public override string ItemName => DataManager.FieldController<TField>().ItemName;

        public FieldColumns<TField> GetFields(SettingsPart part) =>
            fields[part];

        public void SetFields(SettingsPart part, FieldColumns<TField> value) =>
            fields[part].CopyFrom(value);

        private readonly Dictionary<SettingsPart, FieldColumns<TField>> fields = new();
        private readonly ExportSettings<TField, TDAO> exportSettings = new();
        private readonly Filter<TField, TDAO> filter = new(FilterConcat.AND);


        private void AddFields(SettingsPart part)
        {
            FieldColumns<TField> columns = new()
            {
                XmlName = $"{TypeHelper.Name(part).Replace(" ", string.Empty)}Fields"
            };
            AddMember(columns);
            fields.Add(part, columns);
        }

        public override void Init()
        {
            base.Init();
            AddMember(sortings);

            foreach (SettingsPart part in TypeHelper.All<SettingsPart>())
                if (TypeHelper.Helper<SettingsPartHelper>().IsFieldsSettings(part))
                    AddFields(part);

            AddMember(categories);
            AddMember(filter);
            AddMember(exportSettings);
        }

        public override void Clear()
        {
            base.Clear();
            sortings.Clear();
            filter.Clear();
            exportSettings.Clear();
            categories.Clear();

            foreach (var item in fields)
            {
                item.Value.Clear();
                item.Value.CopyFrom(
                    TypeHelper.FieldHelper<TField>()
                        .Columns(
                            FieldVariantHelper.Variant(item.Key),
                            FieldsFilling.Default
                        )
                );
            }
        }

        protected override void LoadData(XmlElement? element)
        {
            base.LoadData(element);

            if (element is null)
                return;

            foreach (XmlNode node in element.ChildNodes)
            {
                DAOSetting setting = TypeHelper.Parse<DAOSetting>(node.Name);

                switch (setting)
                {
                    case DAOSetting.IconClickVariant:
                        settings[setting] = XmlHelper.Value<IconClickVariant>(element, node.Name);
                        break;
                    case DAOSetting.IconsSize:
                        settings[setting] = XmlHelper.Value<IconSize>(element, node.Name);
                        break;
                    case DAOSetting.QuickFilterTextFieldOperation:
                        settings[setting] = XmlHelper.Value<TextFilterOperation>(element, node.Name);
                        break;
                    case DAOSetting.ShowCategories:
                    case DAOSetting.ShowItemInfo:
                    case DAOSetting.ShowQuickFilter:
                        settings[setting] = XmlHelper.Value<FunctionalPanelVisible>(element, node.Name);
                        break;
                    case DAOSetting.ItemInfoPosition:
                        settings[setting] = XmlHelper.Value<ItemInfoPosition>(element, node.Name);
                        break;
                }
            }
        }

        protected override object ParseXMLValue(DAOSetting setting, XmlElement parentElement, string elementName) =>
            setting switch
            {
                DAOSetting.IconClickVariant =>
                    XmlHelper.Value<IconClickVariant>(parentElement, elementName),
                DAOSetting.IconsSize =>
                    XmlHelper.Value<IconSize>(parentElement, elementName),
                DAOSetting.QuickFilterTextFieldOperation =>
                    XmlHelper.Value<TextFilterOperation>(parentElement, elementName),
                _ =>
                    base.ParseXMLValue(setting, parentElement, elementName),
            };

        List<object> IDAOSettings.GetFields(SettingsPart part) =>
            fields[part].ObjectList;

        public void SetFields(SettingsPart part, List<object> value) =>
            fields[part].ObjectList = value;

        public IFieldsPanel CreateFieldsPanel(SettingsPart part, Control parent)
        {
            IFieldsPanel fieldsPanel =
                new FieldsPanel<TField, TDAO>(FieldVariantHelper.Variant(part))
                {
                    Parent = parent,
                    Dock = 
                        part is SettingsPart.QuickFilterText 
                            ? DockStyle.Bottom 
                            : DockStyle.Fill
                };
            fieldsPanel.Margins.SetSize(OxSize.Large);
            fieldsPanel.Paddings.HorizontalOx = OxSize.Medium;

            if (part is SettingsPart.QuickFilterText)
                fieldsPanel.SetContentSize(fieldsPanel.Width, 78);

            return fieldsPanel;
        }

        protected override ISettingsObserver CreateObserver() =>
            new DAOObserver<TField, TDAO>();

        public new DAOObserver<TField, TDAO> Observer => (DAOObserver<TField, TDAO>)base.Observer;

        private bool availableSummary = true;

        public bool AvailableSummary
        {
            get => availableSummary;
            set => availableSummary = value;
        }

        private bool availableQuickFilter = true;

        public bool AvailableQuickFilter
        {
            get => availableQuickFilter;
            set => availableQuickFilter = value;
        }

        private bool availableCategories = true;

        public bool AvailableCategories
        {
            get => availableCategories;
            set => availableCategories = value;
        }

        private bool availableCards = true;

        public bool AvailableCards
        {
            get => availableCards;
            set => availableCards = value;
        }

        private bool availableIcons = true;

        public bool AvailableIcons
        {
            get => availableIcons;
            set => availableIcons = value;
        }

        protected override SystemControlFactory<DAOSetting> CreateControlFactory() =>
            new DAOSettingsControlFactory<TField, TDAO>();

        protected override DAO? CreateDAO(DAOSetting setting) => 
            setting is DAOSetting.IconMapping 
                ? new ListDAO<IconMapping<TField>>() 
                : base.CreateDAO(setting);

        private static DAO DefaultIconMapping()
        {
            FieldHelper<TField> fieldHelper = TypeHelper.FieldHelper<TField>();
            ListDAO<IconMapping<TField>> result = new()
            {
                new()
                {
                    Part = IconContent.Image,
                    Field = fieldHelper.ImageField
                },
                new()
                {
                    Part = IconContent.Title,
                    Field = fieldHelper.TitleField
                }
            };

            return result;
        }

        protected override DAO? DAODefault(DAOSetting setting) => 
            setting switch
            {
                DAOSetting.IconMapping => DefaultIconMapping(),
                _ => base.DAODefault(setting),
            };

        public ICategoriesPanel CreateCategoriesPanel(Control parent)
        {
            ICategoriesPanel categoriesPanel =
                new CategroiesPanel<TField, TDAO>()
                {
                    Parent = parent,
                    Dock = DockStyle.Fill
                };
            categoriesPanel.Margins.SetSize(OxSize.Large);
            categoriesPanel.Paddings.HorizontalOx = OxSize.Medium;
            return categoriesPanel; 
        }
    }
}