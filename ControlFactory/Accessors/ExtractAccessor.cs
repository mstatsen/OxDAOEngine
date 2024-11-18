using OxDAOEngine.ControlFactory.Context;
using OxDAOEngine.ControlFactory.Initializers;
using OxDAOEngine.Data;

namespace OxDAOEngine.ControlFactory.Accessors
{
    public class ExtractAccessor<TField, TDAO> : ComboBoxAccessor<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public ExtractAccessor(FieldContext<TField, TDAO> context, bool forQuickFilter, bool fullExtract)
            : base(
                  context.Initializer is not null 
                    ? context 
                    : context.SetInitializer(
                        new ExtractInitializer<TField, TDAO>(context.Field, addAnyObject: forQuickFilter, fullExtract: fullExtract)
                    )
            ) => 
            ForQuickFilter = forQuickFilter;

        public override void Clear()
        {
            if (ForQuickFilter)
                base.Clear();
            else Value = null;
        }

        private readonly bool ForQuickFilter;
    }
}
