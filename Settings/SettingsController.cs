using OxLibrary.Panels;
using OxDAOEngine.ControlFactory;
using OxDAOEngine.ControlFactory.Accessors;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Types;
using OxDAOEngine.Settings.Helpers;
using OxDAOEngine.Settings.Observers;
using OxDAOEngine.SystemEngine;
using OxDAOEngine.XML;
using System.Xml;
using OxDAOEngine.Settings.Part;
using OxDAOEngine.Data.Fields;

namespace OxDAOEngine.Settings
{
    public abstract class SettingsController<TSetting> : DAO, ISettingsController<TSetting>
        where TSetting : Enum
    {
        public SettingsController() : base() { }

        public override string DefaultXmlElementName => ListName;

        public bool BoolValue(TSetting setting) =>
            BoolValue(settings[setting]);

        public T EnumValue<T>(TSetting setting) =>
            EnumValue<T>(settings[setting]);

        public T? DAOValue<T>(TSetting setting) where T : DAO =>
            DAOValue<T>(settings[setting]);


        public int IntValue(TSetting setting) =>
            IntValue(settings[setting]);

        public object? this[TSetting setting]
        {
            get => settings[setting];
            set
            {
                if (IsDAOSetting(setting))
                {
                    Modified |= value switch
                    {
                        null => 
                            settings[setting] is not null,
                        _ => 
                            !value.Equals(settings[setting]),
                    };

                    if (settings[setting] is DAO dao)
                        dao.CopyFrom((DAO?)value);
                }
                else
                    settings[setting] = ModifyValue(settings[setting], value);
            }
        }

        public object? this[string setting]
        {
            get => this[TypeHelper.Parse<TSetting>(setting)];
            set => this[TypeHelper.Parse<TSetting>(setting)] = value;
        }

        public void Save(XmlElement? parentElement)
        {
            if (parentElement is null)
                return;

            XmlNodeList existingNodes = parentElement.GetElementsByTagName(XmlElementName);

            if (existingNodes.Count > 0)
            {
                XmlNode? firstNode = existingNodes[0];

                if (firstNode is not null)
                    parentElement.RemoveChild(firstNode);
            }

            base.Save(parentElement);
        }

        public string FileName => "Settings.xml";

        public abstract string ListName { get; }
        public abstract string ItemName { get; }

        protected Dictionary<TSetting, object?> settings = new();

        private readonly SettingHelper<TSetting> settingsHelper = 
            TypeHelper.Helper<SettingHelper<TSetting>>();

        public override void Init()
        {
            foreach (TSetting setting in settingsHelper.All())
                if (IsDAOSetting(setting))
                    settings[setting] = CreateDAO(setting);
        }

        protected bool IsDAOSetting(TSetting? setting) => 
            TypeHelper.Helper<SettingHelper<TSetting>>().IsDAOSetting(setting);

        protected virtual DAO? CreateDAO(TSetting setting) => null;

        public ISettingHelper Helper => TypeHelper.Helper<SettingHelper<TSetting>>();
        public SettingHelper<TSetting> SettingHelper => TypeHelper.Helper<SettingHelper<TSetting>>();

        private ISettingsObserver? observer;
        public ISettingsObserver Observer 
        { 
            get
            {
                observer ??= CreateObserver();
                return observer;
            }
        }

        public bool IsSystem => true;

        protected abstract ISettingsObserver CreateObserver();

        public override void Clear()
        {
            foreach (TSetting setting in TypeHelper.All<TSetting>())
                if (IsDAOSetting(setting))
                {
                    if (settings[setting] is DAO dao)
                        dao.CopyFrom(DAODefault(setting));
                }
                else
                    settings[setting] = Helper.Default(setting.ToString());
        }

        public object? GetDefault(TSetting setting) => 
            IsDAOSetting(setting)
                ? DAODefault(setting)
                : Helper.Default(setting.ToString());

        public object? GetDefault(string setting) =>
            GetDefault(TypeHelper.Parse<TSetting>(setting));

        protected virtual DAO? DAODefault(TSetting setting) => null;

        protected override void SaveData(XmlElement element, bool clearModified = true)
        {
            foreach (var item in settings)
            {
                if (IsDAOSetting(item.Key))
                {
                    if (item.Value is DAO dao 
                        && item.Value is not null)
                        dao.Save(element);
                }
                else
                    XmlHelper.AppendElement(element, item.Key.ToString(), item.Value);
            }
        }

        protected bool IsBoolSettings(TSetting setting) => 
            settingsHelper.GetFieldType(setting) == FieldType.Boolean;

        protected bool IsIntSettings(TSetting setting) => 
            settingsHelper.GetFieldType(setting) == FieldType.Integer;

        protected virtual object ParseXMLValue(TSetting setting, XmlElement parentElement, string elementName) => 
            IsBoolSettings(setting)
                ? XmlHelper.ValueBool(parentElement, elementName)
                : IsIntSettings(setting)
                    ? XmlHelper.ValueInt(parentElement, elementName)
                    : XmlHelper.Value(parentElement, elementName);

        protected override void LoadData(XmlElement? element)
        {
            if (element is null)
                return;

            foreach (XmlNode node in element.ChildNodes)
            {
                TSetting? setting = TypeHelper.Parse<TSetting>(node.Name);

                if (IsDAOSetting(setting))
                {
                    if (settings[setting] is DAO dao)
                        dao.Load(element);
                }
                else
                    settings[setting] = ParseXMLValue(setting, element, node.Name);
            }
        }

        public IControlAccessor Accessor(TSetting setting) =>
            ControlFactory.Builder(ControlScope.Editor).Accessor(setting);

        public IControlAccessor Accessor(string setting) =>
            Accessor(TypeHelper.Parse<TSetting>(setting));

        protected abstract SystemControlFactory<TSetting> CreateControlFactory();

        private SystemControlFactory<TSetting>? controlFactory;
        public SystemControlFactory<TSetting> ControlFactory 
        {
            get
            {
                controlFactory ??= CreateControlFactory();
                return controlFactory;
            }
        }

        public event ModifiedChangeHandler? ModifiedHandler;

        public SettingsPart ActiveSettingsPart => SettingsPart.Full;

        public ISettingsController Settings => this;

        public OxPane? Face => null;
        private Bitmap? icon;
        public Bitmap? Icon 
        { 
            get => icon;
            set => icon = value;
        }
    }
}