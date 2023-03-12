﻿using System.Xml;

namespace OxXMLEngine.Data.Filter
{
    public class Filter<TField, TDAO> 
        : DAO, IMatcher<TDAO>
        where TField : notnull, Enum
        where TDAO : DAO, IFieldMapping<TField>, new()
    {
        public Filter(FilterConcat filterConcat = FilterConcat.AND) : base() =>
            Root.FilterConcat = filterConcat;

        public FilterRoot<TField, TDAO> Root { get; internal set; } = new FilterRoot<TField, TDAO>();

        public FilterGroup<TField, TDAO> AddGroup(FilterConcat filterConcat) =>
            Root.AddGroup(filterConcat);

        public SimpleFilter<TField, TDAO> AddFilter(SimpleFilter<TField, TDAO> filter, FilterConcat concatToGroup) =>
            Root.AddFilter(filter, concatToGroup);

        public SimpleFilter<TField, TDAO> AddFilter(TField field, FilterOperation operation, object? value,
            FilterConcat concatToGroup = FilterConcat.OR) =>
            Root.AddFilter(field, operation, value, concatToGroup);

        public SimpleFilter<TField, TDAO> AddFilter(TField field, object value,
            FilterConcat concatToGroup = FilterConcat.OR) =>
            Root.AddFilter(field, value, concatToGroup);

        public bool Match(TDAO? dao) =>
            Root.Match(dao);

        public bool FilterIsEmpty =>
            Root.FilterIsEmpty;

        public override void Clear() =>
            Root.Clear();

        protected override void LoadData(XmlElement element) =>
            Root.Load(element);

        protected override void SaveData(XmlElement element, bool clearModified = true) =>
            Root.Save(element, clearModified);

        public override void Init() { }

        public override bool Equals(object? obj) =>
            obj is Filter<TField, TDAO> otherFilter
            && (base.Equals(obj) || Root.Equals(otherFilter.Root));

        public override int GetHashCode() => 
            Root.GetHashCode();
    }
}