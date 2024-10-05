using OxDAOEngine.Data.Types;

namespace OxDAOEngine.Settings.Part
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

        public static List<SettingsPart> VisibleDAOSettings =>
            new()
            {
                SettingsPart.Table,
                SettingsPart.View,
                SettingsPart.Category,
                SettingsPart.QuickFilter,
                SettingsPart.Summary
            };

        public static List<SettingsPart> VisibleGeneralSettings =>
            new()
            {
                SettingsPart.Main,
                SettingsPart.Styles
            };

        public static List<SettingsPart> FieldsSettings =>
            new()
            {
                SettingsPart.Table,
                SettingsPart.QuickFilter,
                SettingsPart.QuickFilterText,
                SettingsPart.Category,
                SettingsPart.Summary
            };

        public static List<SettingsPart> MandatoryFields =>
            new()
            {
                SettingsPart.Table,
                SettingsPart.QuickFilterText,
            };

        public static bool IsFieldsSettings(SettingsPart part) =>
            FieldsSettings.Contains(part);

        public override SettingsPart EmptyValue() =>
            SettingsPart.Full;
    }
}