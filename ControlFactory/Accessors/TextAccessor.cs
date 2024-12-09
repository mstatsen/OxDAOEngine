using OxLibrary;
using OxLibrary.Controls;
using OxLibrary.Interfaces;
using OxDAOEngine.ControlFactory.Context;
using OxDAOEngine.ControlFactory.ValueAccessors;
using OxDAOEngine.Data;

namespace OxDAOEngine.ControlFactory.Accessors
{
    public class TextAccessor<TField, TDAO> : ControlAccessor<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public TextAccessor(IBuilderContext<TField, TDAO> context) : base(context) { }
        protected override IOxControl CreateControl() => 
            new OxTextBox
            {
                Font = OxStyles.DefaultFont
            };

        protected override ValueAccessor CreateValueAccessor() => 
            new TextBoxValueAccessor();

        protected override void AssignValueChangeHanlderToControl(EventHandler? value) => 
            Control.TextChanged += value;

        protected override void UnAssignValueChangeHanlderToControl(EventHandler? value) => 
            Control.TextChanged -= value;

        public override void Clear() => 
            Value = string.Empty;

        public override bool IsEmpty =>
            base.IsEmpty
            || StringValue.Trim().Equals(string.Empty);
    }
}