using OxXMLEngine.ControlFactory;
using OxXMLEngine.Data.Types;

namespace OxXMLEngine.Settings
{
    public class SettingsPartHelper : AbstractTypeHelper<SettingsPart>
    {
        public override string GetName(SettingsPart part) => 
            part switch
            {
                SettingsPart.Table => "Table",
                SettingsPart.Category => "Categories",
                SettingsPart.QuickFilter => "Quick filter",
                SettingsPart.QuickFilterText => "Text filter",
                SettingsPart.View => "Views",
                SettingsPart.Summary => "Summary",
                SettingsPart.Styles => "Styles",
                SettingsPart.Main => "Main",
                _ => string.Empty,
            };

        public SettingsPartList VisibleDAOSettings =>
            new()
            {
                SettingsPart.Table,
                SettingsPart.View,
                SettingsPart.Category,
                SettingsPart.QuickFilter,
                SettingsPart.Summary
            };

        public SettingsPartList VisibleGeneralSettings =>
            new()
            {
                SettingsPart.Main,
                SettingsPart.Styles
            };

        public SettingsPartList FieldsSettings =>
            new()
            {
                SettingsPart.Table,
                SettingsPart.QuickFilter,
                SettingsPart.QuickFilterText,
                SettingsPart.Category,
                SettingsPart.Summary
            };

        public SettingsPartList MandatoryFields =>
            new()
            {
                SettingsPart.Table,
                SettingsPart.QuickFilterText,
            };

        public bool IsFieldsSettings(SettingsPart part) =>
            FieldsSettings.Contains(part);

        public override SettingsPart EmptyValue() =>
            SettingsPart.Full;
    }
}