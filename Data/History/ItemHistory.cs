﻿using System.Xml;

namespace OxDAOEngine.Data.History
{
    public class ItemHistory<TField, TDAO> : RootDAO<TField>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public virtual DAOOperation Operation { get; }
        public TDAO? DAO { get; set; }

        public ItemHistory() : base() { }

        public ItemHistory(TDAO dao) : this() =>
            DAO = dao;

        public override void Clear() { }

        public override void Init() { }

        protected override object? GetFieldValue(TField field) =>
            DAO?[field];

        protected override void LoadData(XmlElement element) { }

        protected override void SaveData(XmlElement element, bool clearModified = true) { }

        protected override void SetFieldValue(TField field, object? value)
        {
            if (DAO is not null)
                DAO[field] = value;
        }

        protected override void NotifyAboutModify(bool oldValue) { }

        public virtual object? NewValue { get; }
        public virtual object? OldValue { get; }

        public override bool UseImageList => false;
    }
}
