using OxXMLEngine.SystemEngine;
using System.Windows.Forms;

namespace OxXMLEngine.Settings
{
    public class GeneralSettingHelper : SettingHelper<GeneralSetting>
    {
        public override object? Default(GeneralSetting setting) => 
            setting switch
            {
                GeneralSetting.MainFormState =>
                    FormWindowState.Normal,
                GeneralSetting.ShowCustomizeButtons or
                GeneralSetting.ColorizePanels or
                GeneralSetting.DarkerHeaders =>
                    true,
                _ =>
                    null,
            };

        public override string GetName(GeneralSetting value) => 
            value switch
            {
                GeneralSetting.MainFormState => "MainFormState",
                GeneralSetting.ShowCustomizeButtons => "Show customize button on functional panels",
                GeneralSetting.ColorizePanels => "Colorize functional panels",
                GeneralSetting.DarkerHeaders => "Dark functional panels' headers",
                _ => string.Empty,
            };

        public override SettingsPart Part(GeneralSetting setting) => 
            setting switch
            {
                GeneralSetting.ShowCustomizeButtons =>
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
                GeneralSetting.DarkerHeaders => 
                    true,
                _ => 
                    false,
            };
    }
}