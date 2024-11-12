using OxDAOEngine.XML;
using System.Xml;

namespace OxDAOEngine.Data
{
    public abstract class SimpleNameDAO : DAO
    {
        public SimpleNameDAO() : base() =>
            WithoutXmlNode = true;

        private string name = string.Empty;
        public string Name
        {
            get => name;
            set => name = StringValue(ModifyValue(name, value));
        }

        public sealed override void Clear() =>
            Name = string.Empty;

        public sealed override void Init() { }

        protected sealed override void LoadData(XmlElement element) =>
            name = element.InnerText;

        protected sealed override void SaveData(XmlElement element, bool clearModified = true) =>
            XmlHelper.AppendElement(element, XmlElementName, Name);

        public sealed override string ToString() =>
            Name;

        public sealed override bool Equals(object? obj) =>
            obj is SimpleNameDAO otherSimple
            && (base.Equals(obj)
                || Name.Equals(otherSimple.Name));

        public sealed override int GetHashCode() =>
            539060726 + EqualityComparer<string>.Default.GetHashCode(Name);
    }
}
