using Microsoft.VisualBasic;
using OxDAOEngine.Data.Types;
using OxDAOEngine.XML;
using System.Xml;

namespace OxDAOEngine.Data.Links
{
    public class Link<TField> : DAO
        where TField : notnull, Enum
    {
        private string name = string.Empty;
        private string url = string.Empty;

        public Link() =>
            helper = TypeHelper.FieldHelper<TField>()!.GetLinkHelper()!;

        public TField Field { get; private set; } = default!;
        private readonly ILinkHelper<TField> helper = default!;
        public override string DefaultXmlElementName => "Link";

        public string Name
        {
            get => name;
            set => name = StringValue(ModifyValue(name, value));
        }

        public string Url
        {
            get => url;
            set => url = StringValue(ModifyValue(url, value));
        }

        public override void Clear()
        {
            name = string.Empty;
            url = string.Empty;
        }

        public override void Init() { }

        protected override void SaveData(XmlElement element, bool clearModified = true)
        {
            XmlHelper.AppendElement(element, XmlConsts.Name, Name);
            XmlHelper.AppendElement(element, XmlConsts.URL, Url, true);
        }

        protected override void LoadData(XmlElement element)
        {
            name = XmlHelper.Value(element, XmlConsts.Name);
            url = XmlHelper.Value(element, XmlConsts.URL);
        }
        public override string ToString() =>
            Name;

        public override bool Equals(object? obj) =>
            obj is Link<TField> ohterLink
            && (base.Equals(obj)
                || Name.Equals(ohterLink.Name)
                    && Url.Equals(ohterLink.Url));

        public override int GetHashCode() =>
            HashCode.Combine(Name, Url);

        public object Type => helper.Parse(Name);

        public override int CompareTo(DAO? other)
        {
            if (Equals(other))
                return 0;

            Link<TField>? otherLink = (Link<TField>?)other;

            if (otherLink == null)
                return 1;

            List<object> allTypes = helper.All();
            int result = allTypes.IndexOf(Type).CompareTo(allTypes.IndexOf(otherLink.Type));
            return result == 0 ? Name.CompareTo(otherLink.Name) : result;
        }

        public Color LinkColor => TypeHelper.BaseColor(Type);

        public override object ExtractKeyValue => Name;
    }
}