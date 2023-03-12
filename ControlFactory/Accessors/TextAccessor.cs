using OxLibrary.Controls;
using OxXMLEngine.ControlFactory.Context;
using OxXMLEngine.ControlFactory.ValueAccessors;
using OxXMLEngine.Data;

namespace OxXMLEngine.ControlFactory.Accessors
{
    public class TextAccessor<TField, TDAO> : ControlAccessor<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public TextAccessor(IBuilderContext<TField, TDAO> context) : base(context) { }
        protected override Control CreateControl() => 
            new OxTextBox
            {
                Font = EngineStyles.DefaultFont
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
            base.IsEmpty || StringValue.Equals(string.Empty);

        protected override void SetReadOnly(bool value) =>
            TextBox.ReadOnly = value;

        protected override bool GetReadOnly() =>
            TextBox.ReadOnly;
    }
}