using OxDAOEngine.Data.Filter.Types;
using OxDAOEngine.XML;
using System.Xml;

namespace OxDAOEngine.Data.Filter
{
    public class FilterRoot<TField, TDAO> 
        : ListDAO<FilterGroup<TField, TDAO>>, IMatcher<TField>, IMatcherList<TField>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, IFieldMapping<TField>, new()
    {
        public FilterConcat FilterConcat { get; set; } = FilterConcat.AND;

        public List<IMatcher<TField>> MatchList
        {
            get
            {
                List<IMatcher<TField>> matchList = new();
                matchList.AddRange(List);
                return matchList;
            }
        }

        public bool FilterIsEmpty =>
            MatchAggregator<TField>.IsEmpty(this);

        public FilterRoot() =>
            SaveEmptyList = false;

        public FilterGroup<TField, TDAO> AddGroup(FilterConcat filterConcat) => 
            Add(
                new FilterGroup<TField, TDAO>()
                {
                    FilterConcat = filterConcat
                }
            );

        public void Add(Category<TField, TDAO> category)
        {
            foreach (FilterGroup<TField, TDAO> otherListItem in category.Filter.Root)
                if (!otherListItem.FilterIsEmpty)
                    Add(otherListItem);
        }

        public FilterGroup<TField, TDAO> GetSuitableGroup(FilterConcat concatToGroup)
        {
            FilterGroup<TField, TDAO>? group = List.Last();

            return group != null
                && group.FilterConcat == concatToGroup
                ? group
                : AddGroup(concatToGroup);
        }

        public SimpleFilter<TField, TDAO> AddFilter(
            SimpleFilter<TField, TDAO> filter,
            FilterConcat concatToGroup)=>
            GetSuitableGroup(concatToGroup).Add(filter);

        public SimpleFilter<TField, TDAO> AddFilter(TField field, FilterOperation operation, object? value,
            FilterConcat concatToGroup = FilterConcat.OR) =>
            GetSuitableGroup(concatToGroup).Add(field, operation, value);

        public SimpleFilter<TField, TDAO> AddFilter(TField field, object value,
            FilterConcat concatToGroup = FilterConcat.OR) =>
            GetSuitableGroup(concatToGroup).Add(field, value);

        public bool Match(IFieldMapping<TField>? dao) =>
            MatchAggregator<TField>.Match(this, dao);

        protected override void LoadData(XmlElement element)
        {
            base.LoadData(element);
            FilterConcat = XmlHelper.Value<FilterConcat>(element, XmlConsts.Concatenation);
        }

        protected override void SaveData(XmlElement element, bool clearModified = true)
        {
            base.SaveData(element, clearModified);
            XmlHelper.AppendElement(element, XmlConsts.Concatenation, FilterConcat);
        }
    }
}