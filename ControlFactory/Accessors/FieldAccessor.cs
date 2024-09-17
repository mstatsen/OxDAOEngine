using OxDAOEngine.ControlFactory.Context;
using OxDAOEngine.ControlFactory.Initializers;
using OxDAOEngine.Data;

namespace OxDAOEngine.ControlFactory.Accessors
{
    public class FieldAccessor<TField, TDAO, TFieldItem>: EnumAccessor<TField, TDAO, TFieldItem>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
        where TFieldItem : notnull, Enum
    {
        public FieldAccessor(IBuilderContext<TField, TDAO> context) :
            base(context.SetInitializer(new FieldsInitializer<TFieldItem>(context.Scope)))
        { } 

        public List<TFieldItem> ExcludedList
        {
            set
            {
                if (Context.Initializer is FieldsInitializer<TFieldItem> fieldsInitializer)
                {
                    fieldsInitializer.ExcludedValues = value;
                    RenewControl(true);
                }
            }
        }
    }

    public class FieldAccessor<TField, TDAO> : FieldAccessor<TField, TDAO, TField>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public FieldAccessor(IBuilderContext<TField, TDAO> context): base(context) { }
    }
}