using OxDAOEngine.XML;
using System;
using System.Xml;

namespace OxDAOEngine.Data.Filter
{
    public class Category<TField, TDAO> 
        : DAO, IMatcher<TField>
        where TField : notnull, Enum
        where TDAO : DAO, IFieldMapping<TField>, new()
    {
        public readonly Filter<TField, TDAO> Filter = new();
        public FiltrationType Filtration { get; set; } = FiltrationType.StandAlone;

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

        public string Name { get; internal set; } = string.Empty;

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

        public Category<TField, TDAO> AddFilterBlank(TField field,
            FilterConcat concatToGroup = FilterConcat.OR)
        {
            Filter.AddFilter(field, FilterOperation.Blank, null, concatToGroup);
            return this;
        }

        public Category<TField, TDAO> AddFilterNotBlank(TField field,
            FilterConcat concatToGroup = FilterConcat.OR)
        {
            Filter.AddFilter(field, FilterOperation.NotBlank, null, concatToGroup);
            return this;
        }

        public Category<TField, TDAO> AddFilterEquals(TField field, object value,
            FilterConcat concatToGroup = FilterConcat.OR)
        {
            Filter.AddFilter(field, FilterOperation.Equals, value, concatToGroup);
            return this;
        }

        public Category<TField, TDAO> AddFilterNotEquals(TField field, object value,
            FilterConcat concatToGroup = FilterConcat.OR)
        {
            Filter.AddFilter(field, FilterOperation.NotEquals, value, concatToGroup);
            return this;
        }

        public Category<TField, TDAO> AddFilterGreater(TField field, object value,
            FilterConcat concatToGroup = FilterConcat.OR)
        {
            Filter.AddFilter(field, FilterOperation.Greater, value, concatToGroup);
            return this;
        }

        public Category<TField, TDAO> AddFilterLower(TField field, object value,
            FilterConcat concatToGroup = FilterConcat.OR)
        {
            Filter.AddFilter(field, FilterOperation.Lower, value, concatToGroup);
            return this;
        }

        public Category<TField, TDAO> AddFilterContains(TField field, object value,
            FilterConcat concatToGroup = FilterConcat.OR)
        {
            Filter.AddFilter(field, FilterOperation.Contains, value, concatToGroup);
            return this;
        }

        public Category<TField, TDAO> AddFilterNotContains(TField field, object value,
            FilterConcat concatToGroup = FilterConcat.OR)
        {
            Filter.AddFilter(field, FilterOperation.NotContains, value, concatToGroup);
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

                        Filter<TField, TDAO> withParentFilter = new();
                        withParentFilter.Root.FilterConcat = FilterConcat.AND;
                        withParentFilter.Root.Add(this);
                        withParentFilter.Root.Add(ParentCategory);
                        return withParentFilter;
                    case FiltrationType.BaseOnChilds:
                        {
                            Filter<TField, TDAO> byChildsFilter = new();
                            byChildsFilter.Root.FilterConcat = FilterConcat.OR;

                            foreach (Category<TField, TDAO> child in Childs)
                            {
                                byChildsFilter.Root.Add(child);

                                foreach (Category<TField, TDAO> childsChild in child.Childs)
                                    byChildsFilter.Root.Add(childsChild);
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
            Name = XmlHelper.Value(element, XmlConsts.Name);
            Filtration = XmlHelper.Value<FiltrationType>(element, XmlConsts.FiltrationType);
            Filter.Load(element);
            Childs.Load(element);

            foreach (Category<TField, TDAO> child in Childs)
                child.ParentCategory = this;
        }

        protected override void SaveData(XmlElement element, bool clearModified = true)
        {
            XmlHelper.AppendElement(element, XmlConsts.Name, Name);
            XmlHelper.AppendElement(element, XmlConsts.FiltrationType, Filtration);
            Filter.Save(element, clearModified);
            Childs.Save(element, clearModified);
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