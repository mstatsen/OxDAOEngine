using OxDAOEngine.ControlFactory.Controls;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Types;
using OxDAOEngine.Settings.Data;
using OxDAOEngine.SystemEngine;
using OxDAOEngine.View.Types;

namespace OxDAOEngine.Settings.ControlFactory.Controls
{
    public class IconMappingControl<TField> 
        : ListItemsControl<ListDAO<IconMapping<TField>>, IconMapping<TField>, 
            IconMappingEditor<TField>, DAOSetting, SystemRootDAO<DAOSetting>>
        where TField : notnull, Enum
    {
        protected override string GetText() => "Icon Mapping";

        protected override string ItemName() => "Icon Mapping";

        protected override int MaximumItemsCount => 
            TypeHelper.ItemsCount<IconContent>();

        protected override bool EqualsItems(IconMapping<TField>? leftItem, IconMapping<TField>? rightItem) => 
            (leftItem is null 
                && rightItem is null)
            || (leftItem is not null 
                && rightItem is not null 
                && leftItem.Part.Equals(rightItem.Part));
    }
}