using OxDAOEngine.Data.Filter.Types;
using System.Collections;

namespace OxDAOEngine.Data.Filter
{
    public class Filter<TField, TDAO>
        : AbstractFilterPart<TField, TDAO>, IMatcher<TField>, IMatcherList<TField>,
        IEnumerable<FilterGroup<TField, TDAO>>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, IFieldMapping<TField>, new()
    {
        public readonly ListDAO<FilterGroup<TField, TDAO>> Groups = new();

        public Filter() : base() { }

        public Filter(FilterConcat concat) =>
            FilterConcat = concat;

        public override bool IsEmpty => Groups.IsEmpty;

        public List<IMatcher<TField>> MatchList
        {
            get
            {
                List<IMatcher<TField>> matchList = new();
                matchList.AddRange(Groups);
                return matchList;
            }
        }

        public bool FilterIsEmpty =>
            MatchAggregator<TField>.IsEmpty(this);

        public FilterGroup<TField, TDAO> AddGroup(FilterConcat filterConcat) =>
            Groups.Add(
                new FilterGroup<TField, TDAO>()
                {
                    FilterConcat = filterConcat
                }
            );

        public void Add(Category<TField, TDAO> category)
        {
            foreach (FilterGroup<TField, TDAO> otherListItem in category.Filter.Groups)
                if (!otherListItem.FilterIsEmpty)
                    Groups.Add(otherListItem);
        }

        public FilterGroup<TField, TDAO> Add(FilterGroup<TField, TDAO> group) => 
            Groups.Add(group);

        public FilterGroup<TField, TDAO> GetSuitableGroup(FilterConcat concatToGroup)
        {
            FilterGroup<TField, TDAO>? group =
                Count == 0
                    ? null
                    : Groups.Last();

            return 
                group is not null
                && group.FilterConcat == concatToGroup
                    ? group
                    : AddGroup(concatToGroup);
        }

        public FilterRule<TField> AddFilter(
            FilterRule<TField> filter,
            FilterConcat concatToGroup)=>
            GetSuitableGroup(concatToGroup).Add(filter);

        public FilterRule<TField> AddFilter(TField field, FilterOperation operation, object? value,
            FilterConcat concatToGroup = FilterConcat.OR) =>
            GetSuitableGroup(concatToGroup).Add(field, operation, value);

        public FilterRule<TField> AddFilter(TField field, object? value,
            FilterConcat concatToGroup = FilterConcat.OR) =>
            GetSuitableGroup(concatToGroup).Add(field, value);

        public bool Match(IFieldMapping<TField>? dao) =>
            MatchAggregator<TField>.Match(this, dao);

        public int Count => Groups.Count;

        public FilterGroup<TField, TDAO> this[int intex]
        { 
            get => Groups[intex];
            set => Groups[intex] = value;
        }

        public IEnumerator<FilterGroup<TField, TDAO>> GetEnumerator() =>
            Groups.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            Groups.GetEnumerator();

        public override void Clear()
        {
            base.Clear();
            Groups.Clear();
        }
        public override void Init()
        {
            base.Init();
            AddMember(Groups);
        }

        public override bool Equals(object? obj) => 
            base.Equals(obj)
            && obj is Filter<TField, TDAO> otherFilter
            && Groups.Equals(otherFilter.Groups);

        public override int GetHashCode() => 
            base.GetHashCode() ^ Groups.GetHashCode();

        public override string Description
        {
            get
            {
                string result = string.Empty;
                bool severalGroups = Count > 1;

                foreach (FilterGroup<TField, TDAO> group in Groups)
                {
                    string groupDescroption = group.Description;

                    if (severalGroups)
                    {
                        result += "(\n\t";
                        groupDescroption = groupDescroption.Replace("\n", "\n\t");
                    }

                    result += groupDescroption;

                    if (severalGroups)
                        result += "\n)";

                    if (!Groups.Last().Equals(group))
                        result += $"\n{ConcatHelper.Name(FilterConcat)}\n";
                }

                return result;
            }
        }
    }
}