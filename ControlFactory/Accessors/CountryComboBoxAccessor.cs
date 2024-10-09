using OxLibrary.Controls;
using OxDAOEngine.ControlFactory.Context;
using OxDAOEngine.Data;
using OxLibrary.Data.Countries;

namespace OxDAOEngine.ControlFactory.Accessors
{
    public class CountryComboBoxAccessor<TField, TDAO> : ComboBoxAccessor<TField, TDAO, Country, OxCountryComboBox>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        protected override void AfterControlsCreated()
        {
            base.AfterControlsCreated();
            ComboBox.LoadCountries(CountryField.IsPSN, true);
        }

        public CountryComboBoxAccessor(IBuilderContext<TField, TDAO> context) : base(context) { }
    }
}
