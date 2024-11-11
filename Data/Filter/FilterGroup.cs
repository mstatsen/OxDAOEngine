using OxDAOEngine.Data.Filter.Types;
using System.Collections;

namespace OxDAOEngine.Data.Filter
{
    public class FilterGroup<TField, TDAO> : AbstractFilterPart<TField, TDAO>, 
        IMatcher<TField>, IMatcherList<TField>,
        IEnumerable<SimpleFilter<TField, TDAO>>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, IFieldMapping<TField>, new()
    {

        public readonly ListDAO<SimpleFilter<TField, TDAO>> Filters = new();

        public FilterGroup() : base() { }

        public FilterGroup(FilterConcat filterConcat) : base(filterConcat) { }

        public SimpleFilter<TField, TDAO> Add(TField field, FilterOperation operation, object? value) =>
            Filters.Add(new SimpleFilter<TField, TDAO>()
                .AddFilter(field, operation, value)
            );

        public SimpleFilter<TField, TDAO> Add(TField field, object value) =>
            Filters.Add(new SimpleFilter<TField, TDAO>()
                .AddFilter(field, value)
            );

        public SimpleFilter<TField, TDAO> Add(SimpleFilter<TField, TDAO> simpleFilter) =>
            Filters.Add(simpleFilter);

        public bool Match(IFieldMapping<TField>? dao) =>
            MatchAggregator<TField>.Match(this, dao);

        public List<IMatcher<TField>> MatchList
        {
            get
            {
                List<IMatcher<TField>> matchList = new();
                matchList.AddRange(Filters);
                return matchList;
            }
        }

        public bool FilterIsEmpty =>
            MatchAggregator<TField>.IsEmpty(this);

        public override void Init()
        {
            base.Init();
            AddMember(Filters);
        }

        public override void Clear()
        {
            base.Clear();
            Filters.Clear();
        }

        public int Count => Filters.Count;

        public SimpleFilter<TField, TDAO> Current => throw new NotImplementedException();

        public IEnumerator<SimpleFilter<TField, TDAO>> GetEnumerator() =>
            Filters.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            Filters.GetEnumerator();

        public SimpleFilter<TField, TDAO> this[int intex]
        {
            get => Filters[intex];
            set => Filters[intex] = value;
        }
    }
}