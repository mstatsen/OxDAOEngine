using OxLibrary.Panels;
using OxXMLEngine.ControlFactory;
using OxXMLEngine.ControlFactory.Accessors;
using OxXMLEngine.Data;
using OxXMLEngine.Data.Types;
using OxXMLEngine.SystemEngine;
using OxXMLEngine.XML;
using System.Xml;

namespace OxXMLEngine.Settings
{
    public abstract class SettingsController<TSetting> : DAO, ISettingsController<TSetting>
        where TSetting : Enum
    {
        public SettingsController() : base() { }

        public override string DefaultXmlElementName => Name;

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
                            settings[setting] != null,
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
            if (parentElement == null)
                return;

            XmlNodeList existingNodes = parentElement.GetElementsByTagName(XmlElementName);

            if (existingNodes.Count > 0)
            {
                XmlNode? firstNode = existingNodes[0];

                if (firstNode != null)
                    parentElement.RemoveChild(firstNode);
            }

            base.Save(parentElement);
        }

        public string FileName => "Settings.xml";

        public abstract string Name { get; }

        protected Dictionary<TSetting, object?> settings = new();

        public override void Init()
        {
            SettingHelper<TSetting> helper = TypeHelper.Helper<SettingHelper<TSetting>>();

            foreach (TSetting setting in helper.All())
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
                if (observer == null)
                    observer = CreateObserver();

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
                    if (item.Value is DAO dao && item.Value == null)
                        dao.Save(element);
                }
                else
                    XmlHelper.AppendElement(element, item.Key.ToString(), item.Value);
            }
        }

        protected virtual bool IsBoolSettings(TSetting setting) => false;
        protected virtual bool IsIntSettings(TSetting setting) => false;

        protected virtual object ParseXMLValue(TSetting setting, XmlElement parentElement, string elementName)
        {
            if (IsBoolSettings(setting))
                return XmlHelper.ValueBool(parentElement, elementName);
            else
            if (IsIntSettings(setting))
                return XmlHelper.ValueInt(parentElement, elementName);

            return XmlHelper.Value(parentElement, elementName);
        }

        protected override void LoadData(XmlElement? element)
        {
            if (element == null)
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
                if (controlFactory == null)
                    controlFactory = CreateControlFactory();

                return controlFactory;
            }
        }

        public event ModifiedChangeHandler? ModifiedHandler;

        public SettingsPart ActiveSettingsPart => SettingsPart.Full;

        public ISettingsController Settings => this;

        public OxPane? Face => null;
    }
}