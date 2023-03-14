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
                this[fieldHelper.UniqueField] =
                    fieldHelper.GetFieldType(fieldHelper.UniqueField) switch
                    {
                        FieldType.Guid => Guid.NewGuid(),
                        FieldType.Label or FieldType.String => this[fieldHelper.UniqueField] + " (Copy)",
                        FieldType.Memo => "COPY " + this[fieldHelper.UniqueField],
                        _ =>
                            this[fieldHelper.UniqueField]
                    };
            }

            this[fieldHelper.TitleField] += " (Copy)";
        }
    }
}