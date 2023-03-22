using OxLibrary.Controls;
using OxXMLEngine.ControlFactory.Context;
using OxXMLEngine.Data;

namespace OxXMLEngine.ControlFactory.Accessors
{
    public class BoolAccessor<TField, TDAO> : ComboBoxAccessor<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public BoolAccessor(IBuilderContext<TField, TDAO> context) : base(context) { }

        protected override void InitControl()
        {
            OxComboBox comboBox = (OxComboBox)Control;
            comboBox.Items.Clear();
            comboBox.Items.Add(false);
            comboBox.Items.Add(true);
        }
    }
}
