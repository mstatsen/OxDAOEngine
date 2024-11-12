using OxDAOEngine.Data.Filter.Types;
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
                if (BaseOnChilds)
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
                
                return Parent == null ||
                    Parent.BaseOnChilds ||
                    Parent.FilterIsEmpty
                    ? Filter
                    : new Filter<TField, TDAO>(FilterConcat.AND)
                    {
                        this,
                        Parent
                    };
            }
        }

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

        public bool Match(IFieldMapping<TField>? dao) =>
            FullFilter.Match(dao);

        protected override void LoadData(XmlElement element)
        {
            Type = XmlHelper.Value<CategoryType>(element, XmlConsts.Type);
            BaseOnChilds = XmlHelper.ValueBool(element, XmlConsts.BaseOnChilds);
            Name = XmlHelper.Value(element, XmlConsts.Name);

            if (Type == CategoryType.FieldExtraction)
                Field = XmlHelper.Value<TField>(element, XmlConsts.Field);
        }

        protected override void BeforeSave()
        {
            base.BeforeSave();

            if (BaseOnChilds)
                Filter.Clear();
        }

        protected override void SaveData(XmlElement element, bool clearModified = true)
        {
            XmlHelper.AppendElement(element, XmlConsts.Type, Type);
            XmlHelper.AppendElement(element, XmlConsts.BaseOnChilds, BaseOnChilds);

            if (Type == CategoryType.FieldExtraction)
            {
                XmlHelper.AppendElement(element, XmlConsts.Name, $"By {TypeHelper.Name(Field)}");
                XmlHelper.AppendElement(element, XmlConsts.Field, Field);
            }
            else
                XmlHelper.AppendElement(element, XmlConsts.Name, Name);
        }

        public override string ToString() =>
            Name;

        public override bool Equals(object? obj) =>
            base.Equals(obj)
            || ((obj is Category<TField, TDAO> other)
                    && Name.Equals(other.Name)
                    && BaseOnChilds.Equals(other.BaseOnChilds)
                );

        public override int GetHashCode() =>
            base.GetHashCode()
                + Name.GetHashCode()
                + BaseOnChilds.GetHashCode();
    }
}