using OxXMLEngine.XML;
using System;
using System.Collections.Generic;
using System.Xml;

namespace OxXMLEngine.Data.Filter
{
    public class FilterGroups<TField, TDAO>
        : ListDAO<FilterGroup<TField, TDAO>>, IMatcher<TDAO>, IMatcherList<TDAO>
        where TField : notnull, Enum
        where TDAO : DAO, IFieldMapping<TField>, new()
    {
        public bool FilterIsEmpty =>
            MatchAggregator<TDAO>.IsEmpty(this);

        public FilterConcat FilterConcat { get; internal set; } = FilterConcat.AND;

        public List<IMatcher<TDAO>> MatchList
        {
            get
            {
                List<IMatcher<TDAO>> matchList = new();
                matchList.AddRange(List);
                return matchList;
            }
        }

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

        public bool Match(TDAO? dao) =>
            MatchAggregator<TDAO>.Match(this, dao);
    }
}