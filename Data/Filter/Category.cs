﻿using OxDAOEngine.Data.Filter.Types;
using OxDAOEngine.Data.Types;
using OxDAOEngine.XML;
using System.Xml;

namespace OxDAOEngine.Data.Filter
{
    public class Category<TField, TDAO> : TreeItemDAO<Category<TField, TDAO>>, IMatcher<TField>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public string Name { get; internal set; } = string.Empty;
        public CategoryType Type { get; internal set; } = CategoryType.Filter;
        public TField Field { get; internal set; } = default!;
        public bool BaseOnChilds { get; set; } = false;

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

        public Category() : base() { }

        public bool FilterIsEmpty
        {
            get
            {
                bool result = !BaseOnChilds;

                if (!result)
                    return false;

                Category<TField, TDAO>? filteredParent = NearFilteredParent;
                return (Type.Equals(CategoryType.FieldExtraction) || Filter.IsEmpty)
                   && (filteredParent == null || filteredParent.FilterIsEmpty);
            }
        }

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

        public bool IsRootCategory => Parent == null;

        public override void Clear()
        {
            base.Clear();
            Name = string.Empty;
            Filter.Clear();
        }

        public override void Init()
        {
            base.Init();
            AddMember(Filter);
        }

        public bool Match(IFieldMapping<TField>? dao)
        {
            bool result = false;

            if (BaseOnChilds)
                foreach (Category<TField, TDAO> child in Childs)
                {
                    result |= child.Match(dao);

                    if (result)
                        break;
                }
            else
                result =
                    MatchParentFilter(dao)
                    && (FilterIsEmpty || BaseOnChilds || Filter.Match(dao));

            return result;
        }

        public Category<TField, TDAO>? NearFilteredParent
        {
            get
            {
                Category<TField, TDAO>? parent = Parent;

                if (parent == null ||
                    !parent.Filter.IsEmpty)
                    return parent;

                while (parent != null)
                {
                    parent = parent.Parent;

                    if (parent == null ||
                        !parent.Filter.IsEmpty)
                            return parent;
                }

                return null;
           }
        }

        private bool MatchParentFilter(IFieldMapping<TField>? dao)
        {
            Category<TField, TDAO>? filteredParent = NearFilteredParent;
            return filteredParent == null || filteredParent.Match(dao);
        }

        protected override void LoadData(XmlElement element)
        {
            Type = XmlHelper.Value<CategoryType>(element, XmlConsts.Type);
            Name = XmlHelper.Value(element, XmlConsts.Name);
            BaseOnChilds = XmlHelper.ValueBool(element, XmlConsts.BaseOnChilds);

            if (Type == CategoryType.FieldExtraction)
                Field = XmlHelper.Value<TField>(element, XmlConsts.Field);
        }

        protected override void BeforeSave()
        {
            base.BeforeSave();

            if (Type == CategoryType.FieldExtraction)
            {
                BaseOnChilds = false;
                Name = $"By {TypeHelper.Name(Field)}";
            }

            if (Type == CategoryType.FieldExtraction
                || BaseOnChilds)
                Filter.Clear();
        }

        protected override void SaveData(XmlElement element, bool clearModified = true)
        {
            XmlHelper.AppendElement(element, XmlConsts.Type, Type);
            XmlHelper.AppendElement(element, XmlConsts.Name, Name);

            if (Type == CategoryType.FieldExtraction)
                XmlHelper.AppendElement(element, XmlConsts.Field, Field);

            if (BaseOnChilds)
                XmlHelper.AppendElement(element, XmlConsts.BaseOnChilds, BaseOnChilds);
        }

        public override string ToString() =>
            Name;

        public override bool Equals(object? obj) =>
            base.Equals(obj)
            || (obj is Category<TField, TDAO> other
                && Type.Equals(other.Type)
                && Name.Equals(other.Name)
                && BaseOnChilds.Equals(other.BaseOnChilds)
                && Field.Equals(other.Field)
                && Filter.Equals(other.Filter)
                && Childs.Equals(other.Childs)
                );

        public override int GetHashCode() =>
            base.GetHashCode()
                + Name.GetHashCode()
                + BaseOnChilds.GetHashCode();
    }
}