using OxLibrary.Controls;
using OxXMLEngine.ControlFactory.Context;
using OxXMLEngine.ControlFactory.ValueAccessors;
using OxXMLEngine.Data;

namespace OxXMLEngine.ControlFactory.Accessors
{
    public class ImageAccessor<TField, TDAO> : ControlAccessor<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public ImageAccessor(IBuilderContext<TField, TDAO> context) : base(context) { }

        protected override Control CreateControl() => 
            new OxPicture
            {
                Width = 160,
                Height = 78,
                Stretch = true
            };

        protected override ValueAccessor CreateValueAccessor() => 
            new ImageValueAccessor();

        protected override void AssignValueChangeHanlderToControl(EventHandler? value) { }
        protected override void UnAssignValueChangeHanlderToControl(EventHandler? value) { }

        protected override void InitControl() { }

        public override void Clear() => 
            Value = null;

    }
}