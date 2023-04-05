﻿namespace OxXMLEngine.Data.History
{
    public class FieldHistory<TField, TDAO> : ItemHistory<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public override DAOOperation Operation => DAOOperation.Modify;
        private readonly object? oldValue;

        public FieldHistory() : base() { }
        public FieldHistory(TDAO dao) : base(dao) { }
        public FieldHistory(TDAO dao, TField field, object? oldValue) : base(dao)
        {
            Field = field;
            this.oldValue = oldValue;
        }

        public TField Field { get; set; } = default!;

        public override object? NewValue { get => this[Field]; }
        public override object? OldValue { get => oldValue; }
    }
}