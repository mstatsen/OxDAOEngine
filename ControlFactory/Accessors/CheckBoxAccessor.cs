using OxLibrary.Controls;
using OxXMLEngine.ControlFactory.Context;
using OxXMLEngine.ControlFactory.ValueAccessors;
using OxXMLEngine.Data;

namespace OxXMLEngine.ControlFactory.Accessors
{
    public class CheckBoxAccessor<TField, TDAO> : ControlAccessor<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public CheckBoxAccessor(IBuilderContext<TField, TDAO> context) : base(context) { }

        protected override Control CreateControl() => 
            new OxCheckBox()
            {
                CheckAlign = ContentAlignment.MiddleRight,
                Width = 14
            };

        protected override ValueAccessor CreateValueAccessor() =>
            new CheckBoxValueAccessor();

        protected override void AssignValueChangeHanlderToControl(EventHandler? value) =>
            CheckBox.CheckedChanged += value;

        protected override void UnAssignValueChangeHanlderToControl(EventHandler? value) =>
            CheckBox.CheckedChanged -= value;

        public override void Clear() =>
            Value = false;

        public OxCheckBox CheckBox =>
            (OxCheckBox)Control;

        protected override bool GetReadOnly() => 
            CheckBox.ReadOnly;

        protected override void SetReadOnly(bool value) => 
            CheckBox.ReadOnly = value;
    }
}