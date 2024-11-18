using OxDAOEngine.ControlFactory;
using OxDAOEngine.ControlFactory.Accessors;
using OxDAOEngine.ControlFactory.Context;
using OxDAOEngine.ControlFactory.Controls;
using OxDAOEngine.ControlFactory.Initializers;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Filter.Types;
using OxDAOEngine.Settings.ControlFactory.Controls;
using OxDAOEngine.Settings.Data;
using OxDAOEngine.SystemEngine;
using OxDAOEngine.View.Types;

namespace OxDAOEngine.Settings.ControlFactory
{
    public class DAOSettingsControlFactory<TField, TDAO> : SystemControlFactory<DAOSetting>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        protected override IControlAccessor? CreateOtherAccessor(IBuilderContext<DAOSetting, SystemRootDAO<DAOSetting>> context) => 
            context is FieldContext<DAOSetting, SystemRootDAO<DAOSetting>> fieldContext
                ? fieldContext.Field switch
                {
                    DAOSetting.IconsSize => 
                        CreateEnumAccessor<IconSize>(context),
                    DAOSetting.IconClickVariant => 
                        CreateEnumAccessor<IconClickVariant>(context),
                    DAOSetting.IconMapping =>
                        CreateIconMappingAccessor(context),
                    DAOSetting.QuickFilterTextFieldOperation => 
                        CreateEnumAccessor<TextFilterOperation>(context),
                    DAOSetting.ShowCategories or
                    DAOSetting.ShowItemInfo or
                    DAOSetting.ShowQuickFilter =>
                        CreateEnumAccessor<FunctionalPanelVisible>(context),
                    DAOSetting.ItemInfoPosition =>
                        CreateEnumAccessor<ItemInfoPosition>(context),
                    _ => 
                        null,
                }
                : context.Key.Equals("IconMapping:Field")
                    ? new FieldAccessor<DAOSetting, SystemRootDAO<DAOSetting>, TField>(context)
                    : (IControlAccessor?)null;

        private IControlAccessor CreateIconMappingAccessor(IBuilderContext<DAOSetting, SystemRootDAO<DAOSetting>> context)
        {
            IControlAccessor accessor =
                CreateButtonEditAccessor<IconMapping<TField>, ListDAO<IconMapping<TField>>, IconMappingControl<TField>>(context);
            ((ICustomItemsControl<IconMapping<TField>, ListDAO<IconMapping<TField>>>)accessor.Control).FixedItems = 
                (ListDAO<IconMapping<TField>>?)(DataManager.FieldController<TField>().Settings as IDAOSettings<TField>)!
                    .GetDefault("IconMapping");
            return accessor;
        }

        protected override IInitializer? Initializer(IBuilderContext<DAOSetting, SystemRootDAO<DAOSetting>> context) => 
            context is FieldContext<DAOSetting, SystemRootDAO<DAOSetting>> fieldContext
                ? fieldContext.Field switch
                {
                    DAOSetting.CardsPageSize => 
                        new NumericInitializer(1, 18, 3),
                    DAOSetting.IconsPageSize => 
                        new NumericInitializer(1, 120, 15),
                    _ => 
                        base.Initializer(context),
                }
                : base.Initializer(context);
    }
}