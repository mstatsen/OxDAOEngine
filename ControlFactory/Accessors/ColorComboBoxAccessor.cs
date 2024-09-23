using OxLibrary.Controls;
using OxDAOEngine.ControlFactory.Context;
using OxDAOEngine.Data;

namespace OxDAOEngine.ControlFactory.Accessors
{
    public class ColorComboBoxAccessor<TField, TDAO> : ComboBoxAccessor<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        protected override Control CreateControl() =>
            Context.MultipleValue
                ? base.CreateControl() 
                : new OxColorComboBox();

        public OxColorComboBox ColorComboBox =>
            (OxColorComboBox)Control;

        public ColorComboBoxAccessor(IBuilderContext<TField, TDAO> context) : base(context) { }
    }
}
