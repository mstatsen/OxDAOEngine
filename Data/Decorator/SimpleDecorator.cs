﻿using OxDAOEngine.Data.Fields;
using OxDAOEngine.Data.Types;

namespace OxDAOEngine.Data.Decorator
{
    public class SimpleDecorator<TField, TDAO> : Decorator<TField, TDAO>
        where TField : Enum
        where TDAO : RootDAO<TField>, new()
    {
        public SimpleDecorator(TDAO dao) : base(dao) { }

        public override object? Value(TField field) =>
            TypeHelper.FieldHelper<TField>().GetFieldType(field).Equals(FieldType.MetaData) 
                ? null 
                : Dao[field];
    }
}
