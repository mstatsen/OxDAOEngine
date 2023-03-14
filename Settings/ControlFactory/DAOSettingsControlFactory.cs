using OxXMLEngine.ControlFactory.Accessors;
using OxXMLEngine.ControlFactory.Context;
using OxXMLEngine.ControlFactory.Initializers;
using OxXMLEngine.Data;
using OxXMLEngine.Data.Extract;
using OxXMLEngine.Data.Fields;
using OxXMLEngine.Data.Filter;
using OxXMLEngine.Settings.ControlFactory.Controls;
using OxXMLEngine.Settings.Data;
using OxXMLEngine.SystemEngine;
using OxXMLEngine.View;

namespace OxXMLEngine.Settings.ControlFactory
{
    public class DAOSettingsControlFactory<TField> : SystemControlFactory<DAOSetting>
        where TField : notnull, Enum
    {
        protected override FieldType GetFieldControlTypeInternal(DAOSetting field) => 
            field switch
            {
                DAOSetting.HideEmptyCategory or
                DAOSetting.AutoExpandCategories or
                DAOSetting.ShowCategories or
                DAOSetting.ShowItemInfo or
                DAOSetting.ShowIcons or
                DAOSetting.ShowCards =>
                    FieldType.Boolean,
                DAOSetting.CardsPageSize or
                DAOSetting.IconsPageSize =>
                    FieldType.Integer,
                DAOSetting.IconsSize or
                DAOSetting.IconClickVariant or
                DAOSetting.SummarySorting or
                DAOSetting.QuickFilterTextFieldOperation =>
                    FieldType.Enum,
                _ => FieldType.Custom,
            };

        protected override IControlAccessor? CreateOtherAccessor(IBuilderContext<DAOSetting, SystemRootDAO<DAOSetting>> context) => 
            context is FieldContext<DAOSetting, SystemRootDAO<DAOSetting>> fieldContext
                ? fieldContext.Field switch
                {
                    DAOSetting.IconsSize => CreateEnumAccessor<IconSize>(context),
                    DAOSetting.IconClickVariant => CreateEnumAccessor<IconClickVariant>(context),
                    DAOSetting.IconMapping => 
                        CreateButtonEditAccessor<IconMapping<TField>, ListDAO<IconMapping<TField>>, IconMappingControl<TField>>(context),
                    DAOSetting.SummarySorting => CreateEnumAccessor<ExtractCompareType>(context),
                    DAOSetting.QuickFilterTextFieldOperation => CreateEnumAccessor<TextFilterOperation>(context),
                    _ => null,
                }
                : context.Name == "IconMappingField" ? new FieldAccessor<DAOSetting, SystemRootDAO<DAOSetting>, TField>(context) : (IControlAccessor?)null;

        protected override IInitializer? Initializer(IBuilderContext<DAOSetting, SystemRootDAO<DAOSetting>> context) => 
            context is FieldContext<DAOSetting, SystemRootDAO<DAOSetting>> fieldContext
                ? fieldContext.Field switch
                {
                    DAOSetting.CardsPageSize => new NumericInitializer(1, 18, 3),
                    DAOSetting.IconsPageSize => new NumericInitializer(1, 90, 15),
                    _ => base.Initializer(context),
                }
                : base.Initializer(context);
    }
}