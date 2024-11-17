using OxDAOEngine.Data.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public DockStyle Dock(ItemInfoPosition value) =>
            value switch
            { 
                ItemInfoPosition.Bottom => DockStyle.Bottom,
                _ => DockStyle.Right,
            };
    }
}
