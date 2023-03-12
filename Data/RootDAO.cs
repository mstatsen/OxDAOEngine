using OxXMLEngine.Data.Fields;
using OxXMLEngine.Data.Filter;
using OxXMLEngine.Data.Types;

namespace OxXMLEngine.Data
{
    public abstract class RootDAO<TField> : DAO, IFieldMapping<TField>
        where TField : notnull, Enum
    {
        public object? this[TField field]
        {
            get => GetFieldValue(field);
            set => SetFieldValue(field, value);
        }

        protected abstract void SetFieldValue(TField field, object? value);

        protected abstract object? GetFieldValue(TField field);

        public virtual int CompareField(TField field, IFieldMapping<TField> y)
        {
            string? thisString = (this[field] == null) ? string.Empty : this[field]?.ToString();
            return thisString != null ? thisString.CompareTo(y[field]?.ToString()) : y[field] == null ? 0 : -1;
        }

        public virtual object ParseCaldedValue(TField field, string value) =>
            value;
        public FilterOperation DefaultFilterOperation(TField field) =>
            TypeHelper.FieldHelper<TField>().DefaultFilterOperation(field);

        public virtual bool IsCalcedField(TField field) => false;

        protected sealed override void InitUniqueCopy()
        {
            FieldHelper<TField> fieldHelper = TypeHelper.FieldHelper<TField>();

            if (fieldHelper.UniqueField != null)
            {
                switch (fieldHelper.GetFieldType(fieldHelper.UniqueField))
                {
                    case FieldType.Guid:
                        this[fieldHelper.UniqueField] = new Guid();
                        break;
                    case FieldType.Label:
                    case FieldType.String:
                        this[fieldHelper.UniqueField] += " COPY";
                        break;
                    case FieldType.Memo:
                        this[fieldHelper.UniqueField] = "COPY " + this[fieldHelper.UniqueField];
                        break;
                }
            }

            this[fieldHelper.TitleField] += " COPY";
        }
    }
}