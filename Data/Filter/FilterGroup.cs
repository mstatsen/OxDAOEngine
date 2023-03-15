using OxXMLEngine.XML;
using System.Xml;

namespace OxXMLEngine.Data.Filter
{
    public class FilterGroup<TField, TDAO> : ListDAO<SimpleFilter<TField, TDAO>>, 
        IMatcher<TField>, IMatcherList<TField>
        where TField : notnull, Enum
        where TDAO : DAO, IFieldMapping<TField>, new()
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

        public SimpleFilter<TField, TDAO> Add(TField field, FilterOperation operation, object? value)
        {
            SimpleFilter<TField, TDAO> filter = new();
            filter.AddFilter(field, operation, value);
            return Add(filter);
        }

        public SimpleFilter<TField, TDAO> Add(TField field, object value)
        {
            SimpleFilter<TField, TDAO> filter = new();
            filter.AddFilter(field, value);
            return Add(filter);
        }

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