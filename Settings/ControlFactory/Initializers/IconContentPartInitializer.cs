using OxLibrary.Controls;
using OxDAOEngine.ControlFactory.Initializers;
using OxDAOEngine.Data;
using OxDAOEngine.Settings.Data;
using OxDAOEngine.Data.Types;

namespace OxDAOEngine.Settings.ControlFactory.Initializers
{
    public class IconContentPartInitializer<TField> : TypedComboBoxInitializer<IconContent>
        where TField : notnull, Enum
    {
        public ListDAO<IconMapping<TField>>? ExistingMappings;

        public override void InitControl(Control control)
        {
            OxComboBox ComboBox = (OxComboBox)control;

            if (ComboBox.Items.Count > 0)
                ComboBox.SelectedIndex = 0;
        }

        public override bool AvailableValue(IconContent value) =>
            ExistingMappings == null
            || !ExistingMappings.Contains(p => p.Part == value);
    }
}