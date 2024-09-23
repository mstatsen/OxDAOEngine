using OxLibrary.Controls;
using OxDAOEngine.ControlFactory.Context;
using OxDAOEngine.Data;

namespace OxDAOEngine.ControlFactory.Accessors
{
    public class CountryComboBoxAccessor<TField, TDAO> : ComboBoxAccessor<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        protected override Control CreateControl()
        {
            if (Context.MultipleValue)
                return base.CreateControl();
            else
            {
                OxCountryComboBox countryComboBox = new();
                countryComboBox.LoadCounties();
                return countryComboBox;
            }
        }

        public OxCountryComboBox CountryComboBox =>
            (OxCountryComboBox)Control;

        public CountryComboBoxAccessor(IBuilderContext<TField, TDAO> context) : base(context) { }
    }
}
