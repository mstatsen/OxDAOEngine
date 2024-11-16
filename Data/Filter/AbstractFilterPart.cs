using OxDAOEngine.Data.Filter.Types;
using OxDAOEngine.Data.Types;
using OxDAOEngine.XML;
using System.Xml;

namespace OxDAOEngine.Data.Filter
{
    public class AbstractFilterPart<TField, TDAO> : DAO, IWithDescription
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public FilterConcat FilterConcat { get; internal set; } = FilterConcat.AND;
        protected readonly FilterConcatHelper ConcatHelper = TypeHelper.Helper<FilterConcatHelper>();

        public AbstractFilterPart() { }

        public AbstractFilterPart(FilterConcat filterConcat) : this() =>
            FilterConcat = filterConcat;

        protected override void LoadData(XmlElement element) => 
            FilterConcat = XmlHelper.Value<FilterConcat>(element, XmlConsts.Concatenation);

        protected override void SaveData(XmlElement element, bool clearModified = true) => 
            XmlHelper.AppendElement(element, XmlConsts.Concatenation, FilterConcat);

        public override void Init() { }

        public override void Clear() => 
            FilterConcat = FilterConcat.AND;

        public override bool Equals(object? obj) =>
            base.Equals(obj) 
            ||(obj is AbstractFilterPart<TField, TDAO> otherFilter
                && FilterConcat.Equals(otherFilter.FilterConcat)
            );

        public override int GetHashCode() =>
            FilterConcat.GetHashCode();

        public virtual string Description => string.Empty;
    }
}