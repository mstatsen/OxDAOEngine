using OxDAOEngine.ControlFactory.Accessors;
using OxDAOEngine.ControlFactory.Context;
using OxDAOEngine.ControlFactory.Initializers;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Extract;
using OxDAOEngine.Data.Fields;
using OxDAOEngine.Data.Filter;
using OxDAOEngine.Settings.ControlFactory.Controls;
using OxDAOEngine.Settings.Data;
using OxDAOEngine.SystemEngine;
using OxDAOEngine.View;

namespace OxDAOEngine.Settings.ControlFactory
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