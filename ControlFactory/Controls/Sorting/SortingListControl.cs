using OxDAOEngine.Data;
using OxDAOEngine.Data.Sorting;
using OxDAOEngine.Data.Types;
using OxLibrary.Controls;

namespace OxDAOEngine.ControlFactory.Controls.Sorting
{
    public class SortingsControl<TField, TDAO>
        : CustomMovableItemsControl<
            FieldSortings<TField, TDAO>,
            FieldSorting<TField, TDAO>,
            OxListBox,
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