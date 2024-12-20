﻿using OxDAOEngine.Data.Types;

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
                SettingsPart.Cards => "Cards",
                SettingsPart.Icons => "Icons",
                SettingsPart.Styles => "Styles",
                SettingsPart.Main => "Main",
                _ => string.Empty,
            };

        public List<SettingsPart> VisibleDAOSettings =>
            new()
            {
                SettingsPart.Table,
                SettingsPart.Cards,
                SettingsPart.Icons,
                SettingsPart.Category,
                SettingsPart.QuickFilter,
            };

        public List<SettingsPart> VisibleGeneralSettings =>
            new()
            {
                SettingsPart.Main,
                SettingsPart.Styles
            };

        public List<SettingsPart> FieldsSettings =>
            new()
            {
                SettingsPart.Table,
                SettingsPart.QuickFilter,
                SettingsPart.QuickFilterText
            };

        public List<SettingsPart> MandatoryFields =>
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