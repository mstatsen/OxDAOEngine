using OxDAOEngine.Data.Types;
using OxLibrary;

namespace OxDAOEngine.Settings
{
    public class ItemInfoPositionHelper : AbstractTypeHelper<ItemInfoPosition>
    {
        public override ItemInfoPosition EmptyValue() =>
            ItemInfoPosition.Right;

        public override string GetName(ItemInfoPosition value) =>
            value switch
            {
                ItemInfoPosition.Right => "Right",
                ItemInfoPosition.Bottom => "Bottom",
                _ => string.Empty
            };

        public OxDock Dock(ItemInfoPosition value) =>
            value switch
            { 
                ItemInfoPosition.Bottom => OxDock.Bottom,
                _ => OxDock.Right,
            };
    }
}
