using OxLibrary.Controls;
using OxXMLEngine.ControlFactory.Context;
using OxXMLEngine.ControlFactory.ValueAccessors;
using OxXMLEngine.Data;

namespace OxXMLEngine.ControlFactory.Accessors
{
    public class ColorComboBoxAccessor<TField, TDAO> : ComboBoxAccessor<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        protected override ValueAccessor CreateValueAccessor() =>
            Context.MultipleValue
            ? new CheckComboBoxValueAccessor()
            : new SimpleComboBoxValueAccessor();

        protected override Control CreateControl() =>
            Context.MultipleValue
                ? new OxCheckComboBox() 
                : new OxColorComboBox();

        public OxColorComboBox ColorComboBox =>
            (OxColorComboBox)Control;

        public ColorComboBoxAccessor(IBuilderContext<TField, TDAO> context) : base(context) { }
    }
}
