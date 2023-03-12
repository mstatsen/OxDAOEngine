using OxXMLEngine.Data;
using OxXMLEngine.Data.Sorting;
using OxXMLEngine.Data.Types;

namespace OxXMLEngine.ControlFactory.Controls
{
    public class SortingsControl<TField, TDAO>
        : ListMovableItemsControl<
            FieldSortings<TField, TDAO>, 
            FieldSorting<TField, TDAO>, 
            SortingEditor<TField, TDAO>, 
            TField,
            TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        protected override string GetText() => "Sorting";

        protected override int MaximumItemsCount =>
            TypeHelper.FieldHelper<TField>().AvailableFieldsCount(Context.Scope);
    }
}