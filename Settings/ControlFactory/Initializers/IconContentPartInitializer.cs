using OxLibrary.Controls;
using OxXMLEngine.ControlFactory.Initializers;
using OxXMLEngine.Data;
using OxXMLEngine.Settings.Data;
using OxXMLEngine.View;

namespace OxXMLEngine.Settings.ControlFactory.Initializers
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