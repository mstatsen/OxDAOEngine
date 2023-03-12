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
using PlayStationGames.Engine.ControlFactory.Initializers;

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

        protected override IControlAccessor? CreateOtherAccessor(
            IBuilderContext<DAOSetting, SystemRootDAO<DAOSetting>> context)
        {
            if (context is FieldContext<DAOSetting, SystemRootDAO<DAOSetting>> fieldContext)
                switch (fieldContext.Field)
                {
                    case DAOSetting.IconsSize:
                        return CreateEnumAccessor<IconSize>(context);
                    case DAOSetting.IconClickVariant:
                        return CreateEnumAccessor<IconClickVariant>(context);
                    case DAOSetting.IconMapping:
                        return CreateButtonEditAccessor<IconMapping<TField>, 
                            ListDAO<IconMapping<TField>>, IconMappingControl<TField>>(context);
                    case DAOSetting.SummarySorting:
                        return CreateEnumAccessor<ExtractCompareType>(context);
                    case DAOSetting.QuickFilterTextFieldOperation:
                        return CreateEnumAccessor<TextFilterOperation>(context);
                }
            else
            { 
                if (context.Name == "IconMappingField")
                    return new FieldAccessor<DAOSetting, SystemRootDAO<DAOSetting>, TField>(context);
            }

            return null;
        }

        protected override IInitializer? Initializer(IBuilderContext<DAOSetting, SystemRootDAO<DAOSetting>> context)
        {
            if (context is FieldContext<DAOSetting, SystemRootDAO<DAOSetting>> fieldContext)
                switch (fieldContext.Field)
                {
                    case DAOSetting.CardsPageSize:
                        return new NumericInitializer(1, 18, 3);
                    case DAOSetting.IconsPageSize:
                        return new NumericInitializer(1, 90, 15);
                }

            return base.Initializer(context);
        }
    }
}
