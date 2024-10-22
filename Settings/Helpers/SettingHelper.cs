using OxDAOEngine.Data.Types;
using OxDAOEngine.Settings.Part;

namespace OxDAOEngine.Settings.Helpers
{
    public abstract class SettingHelper<TSetting> : AbstractTypeHelper<TSetting>, ISettingHelper
        where TSetting : Enum
    {
        public abstract object? Default(TSetting? setting);

        public abstract SettingsPart Part(TSetting setting);

        public SettingList<TSetting> VisibleSettings
        {
            get
            {
                SettingList<TSetting> result = new();

                foreach (TSetting setting in TypeHelper.All<TSetting>())
                    if (IsVisible(setting))
                        result.Add(setting);

                return result;
            }
        }

        List<string> ISettingHelper.VisibleItems => VisibleSettings.StringList;

        public virtual SettingList<TSetting>? CardSettings => null;

        public virtual SettingList<TSetting>? IconSettings => null;

        public List<string>? CardSettingsItems => CardSettings?.StringList;

        public List<string>? IconSettingsItems => IconSettings?.StringList;

        public SettingList<TSetting> SettingsByPart(SettingsPart part)
        {
            SettingList<TSetting> list = new();

            foreach (TSetting setting in VisibleSettings)
                if (Part(setting) == part)
                    list.Add(setting);

            return list;
        }

        public string Name(string value) => Name(ParseSetting(value));

        protected virtual bool IsVisible(TSetting setting) => true;

        public override TSetting EmptyValue() => default!;

        public virtual int ControlWidth(TSetting? setting) => 60;

        public int ControlWidth(string setting) => ControlWidth(ParseSetting(setting));

        public TSetting ParseSetting(string setting) => TypeHelper.Parse<TSetting>(setting);

        public object? Default(string setting) =>
            Default(ParseSetting(setting));

        public SettingsPart Part(string setting) =>
            Part(ParseSetting(setting));

        List<string> ISettingHelper.ItemsByPart(SettingsPart part) =>
            SettingsByPart(part).StringList;

        public bool IsDAOSetting(string setting) => IsDAOSetting(ParseSetting(setting));

        public virtual bool IsDAOSetting(TSetting? setting) => false;
    }
}