namespace OxXMLEngine.Settings
{
    public class SettingsObserver<TSetting, TSettings> : ISettingsObserver<TSetting>
        where TSetting : Enum
        where TSettings : SettingsController<TSetting>, new()
    {
        public virtual void RenewChanges()
        {
            Clear();

            foreach (TSetting setting in Controller.SettingHelper.All())
            {
                object? oldValue = OldValues[setting];

                if (fullApplies ||
                    (oldValue != null && !oldValue.Equals(Controller[setting])) ||
                    (oldValue == null && Controller[setting] != null)
                    )
                    ChangedSettings.Add(setting);
            }
        }

        protected virtual void Clear() => 
            ChangedSettings.Clear();

        public bool this[TSetting setting] =>
            ChangedSettings.Contains(setting);

        protected TSettings Controller => 
            SettingsManager.Settings<TSettings>();

        public virtual bool IsEmpty =>
            ChangedSettings.Count == 0;

        protected bool fullApplies = false;
        public bool FullApplies 
        { 
            get => fullApplies;
            set
            {
                fullApplies = value;
                RenewChanges();
            }
        }

        public bool this[Enum setting] => this[(TSetting)setting];

        protected readonly SettingList<TSetting> ChangedSettings = new();
        protected readonly TSettings OldValues;

        public SettingsObserver() => 
            OldValues = new TSettings();

        public void RememberState()
        {
            Clear();
            OldValues.CopyFrom(Controller);
        }
    }
}