using OxDAOEngine.Data;
using OxDAOEngine.Data.Types;

namespace OxDAOEngine.ControlFactory.Context
{

    public class FieldAdditionalContext<TField>
    { 
        public readonly TField Field;
        public readonly string TextContext;

        public FieldAdditionalContext(TField field, string textContext)
        {
            Field = field;
            TextContext = textContext;
        }

        public override string ToString() =>
            $"{TypeHelper.Name(Field)}_{TextContext}";
    }


    public class FieldContext<TField, TDAO> : AccessorContext<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public FieldContext(ControlBuilder<TField, TDAO> builder, TField field) : base(
            builder,
            TypeHelper.FullName(field),
            TypeHelper.FullName(field),
            TypeHelper.FieldHelper<TField>().GetFieldType(field)) =>
            Field = field;

        public bool AvailableDependencies { get; set; } = true;

        public TField Field { get; internal set; }
    }
}