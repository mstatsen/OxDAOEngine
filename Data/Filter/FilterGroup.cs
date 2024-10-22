using OxDAOEngine.Data.Filter.Types;
using OxDAOEngine.XML;
using System.Xml;

namespace OxDAOEngine.Data.Filter
{
    public class FilterGroup<TField, TDAO> : ListDAO<SimpleFilter<TField, TDAO>>, 
        IMatcher<TField>, IMatcherList<TField>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, IFieldMapping<TField>, new()
    {
        public FilterConcat FilterConcat { get; internal set; } = FilterConcat.OR;

        public FilterGroup() { }

        protected override void LoadData(XmlElement element)
        {
            base.LoadData(element);
            FilterConcat = XmlHelper.Value<FilterConcat>(element, XmlConsts.FilterConcat);
        }

        protected override void SaveData(XmlElement element, bool clearModified = true)
        {
            base.SaveData(element, clearModified);
            XmlHelper.AppendElement(element, XmlConsts.FilterConcat, FilterConcat);
        }

        public FilterGroup(FilterConcat filterConcat) : this() =>
            FilterConcat = filterConcat;

        public SimpleFilter<TField, TDAO> Add(TField field, FilterOperation operation, object? value) => 
            Add(new SimpleFilter<TField, TDAO>()
                .AddFilter(field, operation, value)
            );

        public SimpleFilter<TField, TDAO> Add(TField field, object value) => 
            Add(new SimpleFilter<TField, TDAO>()
                .AddFilter(field, value)
            );

        public bool Match(IFieldMapping<TField>? dao) =>
            MatchAggregator<TField>.Match(this, dao);

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
    }
}