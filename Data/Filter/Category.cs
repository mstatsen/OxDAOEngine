﻿using OxDAOEngine.Data.Filter.Types;
using OxDAOEngine.Data.Types;
using OxDAOEngine.XML;
using System.Xml;

namespace OxDAOEngine.Data.Filter
{
    public class Category<TField, TDAO> : DAO, IMatcher<TField>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public string Name { get; internal set; } = string.Empty;
        public CategoryType Type { get; internal set; } = CategoryType.Filter;

        public TField Field { get; internal set; } = default!;

        public FiltrationType Filtration { get; set; } = FiltrationType.StandAlone;

        private readonly Filter<TField, TDAO> filter = new(FilterConcat.AND);

        public Filter<TField, TDAO> Filter
        { 
            get => filter;
            set
            {
                filter.Clear();
                filter.CopyFrom(value);
            }
        }

        public Category<TField, TDAO>? ParentCategory { get; set; }

        public readonly Categories<TField, TDAO> Childs = new();

        public Category() : base() { }

        public Category(string name, FiltrationType filtration = FiltrationType.StandAlone) : base()
        {
            Name = name;
            Filtration = filtration;
        }

        public Category<TField, TDAO> AddChild(Category<TField, TDAO> childCategory)
        {
            if (childCategory != null)
            {
                childCategory.ParentCategory = this;
                Childs.Add(childCategory);
            }

            return this;
        }

        public void RemoveChild(Category<TField, TDAO> childCategory) =>
            Childs.Remove(childCategory);

        public bool FilterIsEmpty =>
            FullFilter.FilterIsEmpty;

        public Category<TField, TDAO> AddFilter(TField field, FilterOperation operation, object value,
            FilterConcat concatToGroup = FilterConcat.OR)
        {
            Filter.AddFilter(field, operation, value, concatToGroup);
            return this;
        }

        public Category<TField, TDAO> AddFilter(TField field, object value,
            FilterConcat concatToGroup = FilterConcat.OR)
        {
            Filter.AddFilter(field, value, concatToGroup);
            return this;
        }

        private Filter<TField, TDAO> FullFilter
        {
            get
            {
                switch (Filtration)
                {
                    case FiltrationType.IncludeParent:
                        if (ParentCategory == null ||
                            ParentCategory.Filtration == FiltrationType.BaseOnChilds ||
                            ParentCategory.FilterIsEmpty)
                            return Filter;

                        return new Filter<TField, TDAO>(FilterConcat.AND)
                        {
                            this,
                            ParentCategory
                        };
                    case FiltrationType.BaseOnChilds:
                        {
                            Filter<TField, TDAO> byChildsFilter = new(FilterConcat.OR);

                            foreach (Category<TField, TDAO> child in Childs)
                            {
                                byChildsFilter.Add(child);

                                foreach (Category<TField, TDAO> childsChild in child.Childs)
                                    byChildsFilter.Add(childsChild);
                            }

                            return byChildsFilter;
                        }
                    default:
                        return Filter;
                }
            }
        }

        public override void Clear()
        {
            Name = string.Empty;
            Filter.Clear();
            Childs.Clear();
        }

        public override void Init() =>
            AddMember(Filter);

        public bool Match(IFieldMapping<TField>? dao) =>
            FullFilter.Match(dao);

        protected override void LoadData(XmlElement element)
        {
            Type = XmlHelper.Value<CategoryType>(element, XmlConsts.Type);
            Filtration = XmlHelper.Value<FiltrationType>(element, XmlConsts.Filtration);
            Name = XmlHelper.Value(element, XmlConsts.Name);
            if (Type == CategoryType.FieldExtraction)
                Field = XmlHelper.Value<TField>(element, XmlConsts.Field);

            Childs.Load(element);

            foreach (Category<TField, TDAO> child in Childs)
                child.ParentCategory = this;
        }

        protected override void SaveData(XmlElement element, bool clearModified = true)
        {
            XmlHelper.AppendElement(element, XmlConsts.Type, Type);
            XmlHelper.AppendElement(element, XmlConsts.Filtration, Filtration);

            if (Type == CategoryType.FieldExtraction)
            {
                XmlHelper.AppendElement(element, XmlConsts.Name, $"By {TypeHelper.Name(Field)}");
                XmlHelper.AppendElement(element, XmlConsts.Field, Field);
            }
            else
            {
                XmlHelper.AppendElement(element, XmlConsts.Name, Name);
            }
        }

        public override string ToString() =>
            Name;

        public override bool Equals(object? obj) =>
            base.Equals(obj)
            || ((obj is Category<TField, TDAO> other)
                    && Name.Equals(other.Name)
                    && Filtration.Equals(other.Filtration)
                );

        public override int GetHashCode() =>
            base.GetHashCode()
                + Name.GetHashCode()
                + Filtration.GetHashCode();
    }
}