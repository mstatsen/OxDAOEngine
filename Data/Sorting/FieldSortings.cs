using OxXMLEngine.Data.Filter;
using OxXMLEngine.XML;

namespace OxXMLEngine.Data.Sorting
{
    public class FieldSortings<TField, TDAO> : ListDAO<FieldSorting<TField, TDAO>>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public override string DefaultXmlElementName => XmlConsts.Sortings;

        public void Clear(SortingVariant variant)
        {
            if (variant == SortingVariant.GroupBy)
                Clear();
            else
                ResetToDefault();
        }

        public List<ISorting<TField, TDAO>> SortingsList
        {
            get
            {
                List<ISorting<TField, TDAO>> result = new();
                result.AddRange(this);
                return result;
            }
            set
            {
                Clear();

                foreach (ISorting<TField, TDAO> sorting in value)
                    if (sorting is FieldSorting<TField, TDAO> fieldSorting)
                        Add(fieldSorting.Field, fieldSorting.SortOrder);

                Modified = true;
            }
        }

        public void ResetToDefault()
        {
            List<ISorting<TField, TDAO>>? defaultSorting = DataManager.DefaultSorting<TField, TDAO>()?.SortingsList;

            if (defaultSorting != null)
                SortingsList = defaultSorting;
            else
                Clear();
        }

        public List<TField> Fields
        {
            get
            {
                List<TField> result = new();

                foreach (FieldSorting<TField, TDAO> item in List)
                    result.Add(item.Field);

                return result;
            }
        }

        public void Add(TField field, SortOrder sortOrder)
        {
            FieldSorting<TField, TDAO>? sorting = Find((s) => s.Field.Equals(field));

            if (sorting != null && sortOrder == SortOrder.None)
                Remove(sorting);
            else
            if (sorting != null)
                sorting.SortOrder = sortOrder;
            else
                Add(new FieldSorting<TField, TDAO>(field, sortOrder));
        }

        protected override bool AutoSorting =>
            false;
    }
}
