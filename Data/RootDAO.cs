using OxXMLEngine.Data.Fields;
using OxXMLEngine.Data.Filter;
using OxXMLEngine.Data.Types;

namespace OxXMLEngine.Data
{
    public class FieldModifiedEventArgs<TField> : EventArgs
        where TField : notnull, Enum
    { 
        public RootDAO<TField> DAO { get; }
        public TField Field { get; }
        public object? OldValue { get; }

        public FieldModifiedEventArgs(RootDAO<TField> dao, TField field, object? oldValue)
        {
            DAO = dao;
            Field = field;
            OldValue = oldValue;
        }
    }

    public delegate void FieldModified<TField>(FieldModifiedEventArgs<TField> e)
        where TField : notnull, Enum;

    public abstract class RootDAO<TField> : DAO, IFieldMapping<TField>
        where TField : notnull, Enum
    {

        public FieldModified<TField>? FieldModified;

        public FieldHelper<TField> FieldHelper = TypeHelper.FieldHelper<TField>();

        public object? this[TField field]
        {
            get => GetFieldValue(field);
            set => SetFieldValue(field, value);
        }

        protected Dictionary<TField, DAO> FieldMembers = new();

        protected void AddMember(TField field, DAO member)
        {
            AddMember(member);
            FieldMembers.Add(field, member);
        }

        protected T? ModifyValue<T>(TField field, T? oldValue, T? newValue)
        {
            if (CheckValueModified(oldValue, newValue))
            {
                OnFieldModified(new FieldModifiedEventArgs<TField>(this, field, oldValue));
                Modified = true;
            }
            
            return newValue;
        }

        protected override void MemberModifiedHandler(DAO dao, DAOModifyEventArgs e)
        {
            if (e.Modified)
                foreach (KeyValuePair<TField, DAO> item in FieldMembers)
                    if (item.Value == dao)
                        OnFieldModified(new FieldModifiedEventArgs<TField>(this, item.Key, e.OldValue));

            base.MemberModifiedHandler(dao, e);
        }
            

        protected virtual void OnFieldModified(FieldModifiedEventArgs<TField> e) => 
            FieldModified?.Invoke(e);

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