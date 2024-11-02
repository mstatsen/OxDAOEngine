using OxDAOEngine.Data;
using OxDAOEngine.Data.Filter;

namespace OxDAOEngine.ControlFactory.Controls.Links
{
    public class CategoriesControl<TField, TDAO> : 
        ListItemsControl<Categories<TField, TDAO>, Category<TField, TDAO>, CategoryEditor<TField, TDAO>, TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        protected override string ItemName() => "Category";

        protected override string GetText() => "Categories";
    }
}