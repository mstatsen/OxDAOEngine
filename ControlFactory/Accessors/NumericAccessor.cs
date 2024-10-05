using OxLibrary.Controls;
using OxDAOEngine.ControlFactory.Context;
using OxDAOEngine.ControlFactory.ValueAccessors;
using OxDAOEngine.Data;

namespace OxDAOEngine.ControlFactory.Accessors
{
    public class NumericAccessor<TField, TDAO> : ControlAccessor<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public NumericAccessor(IBuilderContext<TField, TDAO> context) : base(context) { }

        protected override Control CreateControl() => 
            new OxSpinEdit();

        protected override ValueAccessor CreateValueAccessor() => 
            new NumericValueAccessor();

        protected override void UnAssignValueChangeHanlderToControl(EventHandler? value) =>
            SpinEdit.ValueChanged -= value;

        protected override void AssignValueChangeHanlderToControl(EventHandler? value) =>
            SpinEdit.ValueChanged += value;

        public OxSpinEdit SpinEdit => (OxSpinEdit)Control;

        public override void ClearValueConstraints()
        {
            MaximumValue = 500;
            MinimumValue = 0;
        }

        public override void Clear() => 
            Value = MinimumValue;

        public override object? MaximumValue 
        { 
            get => SpinEdit.Maximum;
            set => SpinEdit.Maximum = DAO.IntValue(value);
        }

        public int Step
        {
            get => SpinEdit.Step;
            set => SpinEdit.Step = Math.Max(DAO.IntValue(value), 1);
        }

        public override object? MinimumValue 
        { 
            get => SpinEdit.Minimum;
            set => SpinEdit.Minimum = DAO.IntValue(value);
        }

        public bool ShowStepButtons
        {
            get => SpinEdit.ShowStepButtons;
            set => SpinEdit.ShowStepButtons = value;
        }
    }
}