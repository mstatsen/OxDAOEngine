using OxXMLEngine.Data;
using OxXMLEngine.Data.Fields;
using OxXMLEngine.Data.Types;

namespace OxXMLEngine.ControlFactory.Controls
{
    public class FieldsControl<TField, TDAO>
        : ListMovableItemsControl<FieldColumns<TField>, FieldColumn<TField>, FieldEditor<TField, TDAO>, TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        protected override string GetText() => "Fields";

        protected override int MaximumItemsCount =>
            TypeHelper.FieldHelper<TField>().AvailableFieldsCount(Context.Scope);
    }
}