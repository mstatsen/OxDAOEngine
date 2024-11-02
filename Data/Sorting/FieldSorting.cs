using OxDAOEngine.Data.Filter;
using OxDAOEngine.Data.Types;
using OxDAOEngine.XML;
using System.Xml;

namespace OxDAOEngine.Data.Sorting
{
    public class FieldSorting<TField, TDAO> : DAO, ISorting<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public FieldSorting() { }

        public override string DefaultXmlElementName => XmlConsts.Sorting;

        public FieldSorting(TField field, SortOrder sortOrder) : this()
        {
            Field = field;
            SortOrder = sortOrder;
        }

        public override bool IsEmpty =>
            base.IsEmpty || Field.Equals(TypeHelper.EmptyValue<TField>());

        public TField Field { get; set; } = default!;

        public SortOrder SortOrder { get; set; }
        public object FieldAsObject
        {
            get => Field;
            set => Field = (TField)value;
        }

        public override void Clear()
        {
            SortOrder = SortOrder.Ascending;
            Field = TypeHelper.DefaultValue<TField>();
        }

        public override int CompareTo(IDAO? other)
        {
            FieldSorting<TField, TDAO>? otherSorting = (FieldSorting<TField, TDAO>?)other;
            return otherSorting == null ? 1 : Field.CompareTo(otherSorting.Field);
        }

        public override bool Equals(object? obj) =>
            obj is FieldSorting<TField, TDAO> sorting
                && (base.Equals(obj) ||
                    Field.Equals(sorting.Field) && SortOrder == sorting.SortOrder);

        public override int GetHashCode() =>
            HashCode.Combine(Field, SortOrder);

        public override void Init() { }

        public override string ToString() =>
            $"{TypeHelper.FullName(Field)} ({TypeHelper.ShortName(SortOrder)})";

        protected override void LoadData(XmlElement element)
        {
            Field = XmlHelper.Value<TField>(element, XmlConsts.Field);
            SortOrder = XmlHelper.Value<SortOrder>(element, XmlConsts.SortOrder);
        }

        protected override void SaveData(XmlElement element, bool clearModified = true)
        {
            XmlHelper.AppendElement(element, XmlConsts.Field, Field);
            XmlHelper.AppendElement(element, XmlConsts.SortOrder, SortOrder);
        }

        public int Compare(TDAO? x, TDAO? y)
        {
            var result = x switch
            {
                null => y == null ? 0 : -1,
                _ => y == null ? 1 : x.CompareField(Field, y),
            };

            if (result != 0 &&
                SortOrder == SortOrder.Descending)
                result = -result;

            return result;
        }
    }
}