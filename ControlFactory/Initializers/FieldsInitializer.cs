using OxDAOEngine.Data.Types;

namespace OxDAOEngine.ControlFactory.Initializers
{
    public class FieldsInitializer<TField> : TypedComboBoxInitializer<TField>
        where TField : notnull, Enum
    {
        private ControlScope Scope;
        public List<TField>? ExcludedValues { get; set; }

        public override bool AvailableValue(TField value) =>
            (ExcludedValues == null || !ExcludedValues.Contains(value))
            && TypeHelper.FieldHelper<TField>().AvailableField(Scope, value);

        public override void InitControl(Control control) => 
            ((ComboBox)control).Sorted = true;

        public void SetControlScope(ControlScope scope) =>
            Scope = scope;

        public FieldsInitializer(ControlScope scope) =>
            Scope = scope;
    }
}