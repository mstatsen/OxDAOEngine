using OxDAOEngine.Data;
using OxDAOEngine.Data.Types;

namespace OxDAOEngine.ControlFactory.Context
{
    public class FieldContext<TField, TDAO> : AccessorContext<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public FieldContext(ControlBuilder<TField, TDAO> builder, TField field): base(
            builder, 
            TypeHelper.FullName(field), 
            builder.Factory.GetFieldControlType(field)) => 
            Field = field;

        public bool AvailableDependencies { get; set; } = true;

        public TField Field { get; internal set; }
    }
}