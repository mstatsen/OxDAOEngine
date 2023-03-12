using OxLibrary.Controls;
using OxXMLEngine.ControlFactory.Context;
using OxXMLEngine.ControlFactory.ValueAccessors;
using OxXMLEngine.Data;

namespace OxXMLEngine.ControlFactory.Accessors
{
    public class LabelAccessor<TField, TDAO> : ControlAccessor<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public LabelAccessor(IBuilderContext<TField, TDAO> context) : base(context) { }
        protected override Control CreateControl() =>
            new OxLabel
            {
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = true
            };

        protected override ValueAccessor CreateValueAccessor() =>
            new LabelValueAccessor();

        public override void Clear() => 
            Value = string.Empty;
    }
}
