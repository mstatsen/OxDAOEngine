using OxLibrary.Controls;
using OxDAOEngine.ControlFactory.Context;
using OxDAOEngine.ControlFactory.ValueAccessors;
using OxDAOEngine.Data;

namespace OxDAOEngine.ControlFactory.Accessors
{
    public class CountryComboBoxAccessor<TField, TDAO> : ComboBoxAccessor<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        protected override ValueAccessor CreateValueAccessor() =>
            Context.MultipleValue
            ? new CheckComboBoxValueAccessor()
            : new SimpleComboBoxValueAccessor();

        protected override Control CreateControl()
        {
            if (Context.MultipleValue)
            {
                return new OxCheckComboBox();
            }
            else
            {
                OxCountryComboBox countryComboBox = new();
                countryComboBox.LoadCounties();
                return countryComboBox;
            }
        }

        public OxCountryComboBox ColorComboBox =>
            (OxCountryComboBox)Control;

        public CountryComboBoxAccessor(IBuilderContext<TField, TDAO> context) : base(context) { }
    }
}
