using OxDAOEngine.Data.Filter.Types;

namespace OxDAOEngine.Data.Filter
{
    public interface ICategory<TField> : IDAO, IMatcher<TField>
        where TField : notnull, Enum
    {
        IMatcher<TField> Filter { get; set; }

        ICategory<TField>? ParentCategory { get; set; }

        IListDAO Childs { get; set; }

        ICategory<TField> AddChild(ICategory<TField> childCategory);

        void RemoveChild(ICategory<TField> childCategory);

        string Name { get; set; }

        public ICategory<TField> AddFilter(TField field, FilterOperation operation, object value,
            FilterConcat concatToGroup = FilterConcat.OR);

        ICategory<TField> AddFilter(TField field, object value,
            FilterConcat concatToGroup = FilterConcat.OR);

        public ICategory<TField> AddFilterBlank(TField field,
            FilterConcat concatToGroup = FilterConcat.OR);

        ICategory<TField> AddFilterNotBlank(TField field,
            FilterConcat concatToGroup = FilterConcat.OR);

        ICategory<TField> AddFilterEquals(TField field, object value,
            FilterConcat concatToGroup = FilterConcat.OR);

        ICategory<TField> AddFilterNotEquals(TField field, object value,
            FilterConcat concatToGroup = FilterConcat.OR);

        ICategory<TField> AddFilterGreater(TField field, object value,
            FilterConcat concatToGroup = FilterConcat.OR);

        ICategory<TField> AddFilterLower(TField field, object value,
            FilterConcat concatToGroup = FilterConcat.OR);

        ICategory<TField> AddFilterContains(TField field, object value,
            FilterConcat concatToGroup = FilterConcat.OR);

        ICategory<TField> AddFilterNotContains(TField field, object value,
            FilterConcat concatToGroup = FilterConcat.OR);

        IMatcher<TField> FullFilter { get; }

        string ToString();

        bool Equals(object? obj);

        int GetHashCode();
    }
}