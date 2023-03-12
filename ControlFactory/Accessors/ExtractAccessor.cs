using OxXMLEngine.ControlFactory.Context;
using OxXMLEngine.ControlFactory.Initializers;
using OxXMLEngine.Data;

namespace OxXMLEngine.ControlFactory.Accessors
{
    public class ExtractAccessor<TField, TDAO> : ComboBoxAccessor<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public ExtractAccessor(FieldContext<TField, TDAO> context, bool forQuickFilter, bool fullExtract)
            : base(context.SetInitializer(
                new ExtractInitializer<TField, TDAO>(context.Field, forQuickFilter, fullExtract)
            )) 
        {
            ForQuickFilter = forQuickFilter;
        }

        public override void Clear()
        {
            if (ForQuickFilter)
                base.Clear();
            else Value = null;
        }

        private readonly bool ForQuickFilter;
    }
}
