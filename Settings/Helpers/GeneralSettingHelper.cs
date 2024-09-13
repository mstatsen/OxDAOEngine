using OxXMLEngine.Data;

namespace OxXMLEngine.Settings.Helpers
{
    public class GeneralSettingHelper : SettingHelper<GeneralSetting>
    {
        public override object? Default(GeneralSetting setting) =>
            setting switch
            {
                GeneralSetting.CurrentController =>
                    DataManager.FirstFieldController(),
                GeneralSetting.MainFormState =>
                    FormWindowState.Normal,
                GeneralSetting.ShowCustomizeButtons or
                GeneralSetting.ColorizePanels or
                GeneralSetting.DarkerHeaders =>
                    true,
                GeneralSetting.DoublePinButtons =>
                    false,
                _ =>
                    null,
            };

        public override string GetName(GeneralSetting value) =>
            value switch
            {
                GeneralSetting.CurrentController => "Current Controller",
                GeneralSetting.MainFormState => "MainFormState",
                GeneralSetting.ShowCustomizeButtons => "Show customize button on functional panels",
                GeneralSetting.ColorizePanels => "Colorize functional panels",
                GeneralSetting.DarkerHeaders => "Dark functional panels' headers",
                GeneralSetting.DoublePinButtons => "Two Pin buttons for pinnable functional panels",
                _ => string.Empty,
            };

        public override SettingsPart Part(GeneralSetting setting) =>
            setting switch
            {
                GeneralSetting.ShowCustomizeButtons or
                GeneralSetting.DoublePinButtons =>
                    SettingsPart.Main,
                GeneralSetting.ColorizePanels or
                GeneralSetting.DarkerHeaders =>
                    SettingsPart.Styles,
                _ =>
                    SettingsPart.Full,
            };

        protected override bool IsVisible(GeneralSetting setting) =>
            setting switch
            {
                GeneralSetting.ShowCustomizeButtons or
                GeneralSetting.ColorizePanels or
                GeneralSetting.DarkerHeaders or
                GeneralSetting.DoublePinButtons =>
                    true,
                _ =>
                    false,
            };
    }
}