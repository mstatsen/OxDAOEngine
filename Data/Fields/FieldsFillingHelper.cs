using System.Drawing;
using OxLibrary;
using OxLibrary.Controls;
using OxDAOEngine.Data.Types;

namespace OxDAOEngine.Data.Fields
{
    public class FieldsFillingHelper : AbstractTypeHelper<FieldsFilling>
    {
        public override FieldsFilling EmptyValue() => FieldsFilling.Full;

        public override string GetName(FieldsFilling value) => 
            value switch
            {
                FieldsFilling.Full => "All",
                FieldsFilling.Default => "Default",
                FieldsFilling.Min => "Min",
                FieldsFilling.Clear => "Clear",
                _ => string.Empty,
            };

        public override string GetFullName(FieldsFilling value) =>
            value switch
            {
                FieldsFilling.Full => "Add all fields",
                FieldsFilling.Default => "Add default field set",
                FieldsFilling.Min => "Add minimum field set",
                FieldsFilling.Clear => "Clear list",
                _ => string.Empty,
            };

        public static int ButtonWidth(FieldsFilling value) => 
            value switch
            {
                FieldsFilling.Full or 
                FieldsFilling.Min => 
                    50,
                _ => 
                    80,
            };

        public static Bitmap ButtonIcon(FieldsFilling value) =>
            value switch
            {
                FieldsFilling.Clear => OxIcons.Eraser,
                _ => OxIcons.Plus,
            };
    }
}