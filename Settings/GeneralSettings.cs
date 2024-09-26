using OxDAOEngine.Settings.ControlFactory;
using OxDAOEngine.Settings.Observers;
using OxDAOEngine.SystemEngine;

namespace OxDAOEngine.Settings
{
    public class GeneralSettings : SettingsController<GeneralSetting>
    {
        public override string ListName => "General";
        public override string ItemName => "General";

        public string CurrentController
        {
            get => (this[GeneralSetting.CurrentController]?.ToString()) ?? string.Empty;
            set => this[GeneralSetting.CurrentController] = value;
        }

        public FormWindowState MainFormState
        {
            get => this[GeneralSetting.MainFormState]?.ToString() == "Maximized"
                        ? FormWindowState.Maximized
                        : FormWindowState.Normal;
            set => this[GeneralSetting.MainFormState] =
                    value == FormWindowState.Maximized
                        ? "Maximized"
                        : "Normal";
        }

        public bool ShowCustomizeButtons
        {
            get => BoolValue(GeneralSetting.ShowCustomizeButtons);
            set => settings[GeneralSetting.ShowCustomizeButtons] = value;
        }

        public bool ColorizePanels
        {
            get => BoolValue(GeneralSetting.ColorizePanels);
            set => settings[GeneralSetting.ColorizePanels] = value;
        }

        public bool DarkerHeaders
        {
            get => BoolValue(GeneralSetting.DarkerHeaders);
            set => settings[GeneralSetting.DarkerHeaders] = value;
        }

        public bool DoublePinButtons
        {
            get => BoolValue(GeneralSetting.DoublePinButtons);
            set => settings[GeneralSetting.DoublePinButtons] = value;
        }

        protected override ISettingsObserver CreateObserver() => 
            new SettingsObserver<GeneralSetting, GeneralSettings>();

        protected override bool IsBoolSettings(GeneralSetting setting)
        {
            return setting switch
            {
                GeneralSetting.ShowCustomizeButtons or 
                GeneralSetting.ColorizePanels or 
                GeneralSetting.DarkerHeaders or
                GeneralSetting.DoublePinButtons => 
                    true,
                _ => 
                    base.IsBoolSettings(setting),
            };
        }

        protected override SystemControlFactory<GeneralSetting> CreateControlFactory() =>
            new GeneralSettingsControlFactory();
    }
}