using OxLibrary.Controls;
using OxDAOEngine.ControlFactory.Context;
using OxDAOEngine.ControlFactory.ValueAccessors;
using OxDAOEngine.Data;
using OxLibrary;

namespace OxDAOEngine.ControlFactory.Accessors
{
    public class TextAccessor<TField, TDAO> : ControlAccessor<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public TextAccessor(IBuilderContext<TField, TDAO> context) : base(context) { }
        protected override Control CreateControl() => 
            new OxTextBox
            {
                Font = Styles.DefaultFont
            };

        protected override ValueAccessor CreateValueAccessor() => 
            new TextBoxValueAccessor();

        protected override void AssignValueChangeHanlderToControl(EventHandler? value) => 
            TextBox.TextChanged += value;

        protected override void UnAssignValueChangeHanlderToControl(EventHandler? value) => 
            TextBox.TextChanged -= value;

        public override void Clear() => 
            Value = string.Empty;

        public OxTextBox TextBox => 
            (OxTextBox)Control;

        public override bool IsEmpty =>
            base.IsEmpty || StringValue.Trim().Equals(string.Empty);
    }
}