using OxDAOEngine.Data;
using OxDAOEngine.Data.Filter;

namespace OxDAOEngine.ControlFactory.Filter
{
    public delegate void ChangeCategoryHandler<TField, TDAO>(object? sender, CategoryEventArgs<TField, TDAO> eventArgs)
            where TField : notnull, Enum
            where TDAO : RootDAO<TField>, new();
    public class CategoryEventArgs<TField, TDAO> : EventArgs
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    { 
        public Category<TField, TDAO>? OldCategory { get; private set; }
        public Category<TField, TDAO>? NewCategory { get; private set; }

        public CategoryEventArgs(Category<TField, TDAO>? oldCategory, Category<TField, TDAO>? newCategory)
        {
            OldCategory = oldCategory;
            NewCategory = newCategory;
        }

        public bool IsFilterChanged
        {
            get
            {
                if (OldCategory is null)
                    return NewCategory is not null;

                if (NewCategory is null)
                    return true;

                return !OldCategory.Filter.Equals(NewCategory.Filter)
                    || (OldCategory.NearFilteredParent is null 
                        && NewCategory.NearFilteredParent is not null)
                    || (OldCategory.NearFilteredParent is not null
                        && !OldCategory.NearFilteredParent.Equals(NewCategory.NearFilteredParent));
            }
        }
    }
}