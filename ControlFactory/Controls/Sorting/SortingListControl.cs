using OxDAOEngine.Data;
using OxDAOEngine.Data.Sorting;
using OxDAOEngine.Data.Types;

namespace OxDAOEngine.ControlFactory.Controls.Sorting
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

        protected override string ItemName() => "Sorting field";

        protected override int MaximumItemsCount =>
            TypeHelper.FieldHelper<TField>().AvailableFieldsCount(Context.Scope);
    }
}