using OxLibrary.Controls;
using OxDAOEngine.ControlFactory.Context;
using OxDAOEngine.Data;

namespace OxDAOEngine.ControlFactory.Accessors
{
    public class ColorComboBoxAccessor<TField, TDAO> : ComboBoxAccessor<TField, TDAO, string, OxColorComboBox>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public ColorComboBoxAccessor(IBuilderContext<TField, TDAO> context) : base(context) { }
    }
}
