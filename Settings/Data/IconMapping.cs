using OxXMLEngine.Data;
using OxXMLEngine.Data.Types;
using OxXMLEngine.View;
using OxXMLEngine.XML;
using System.Xml;

namespace OxXMLEngine.Settings.Data
{
    public class IconMapping<TField> : DAO
        where TField : notnull, Enum
    {
        private TField field = default!;

        public TField Field
        {
            get => field;
            set => field = EnumValue<TField>(ModifyField(field, value));
        }

        private IconContent part = default;

        public IconContent Part
        {
            get => part;
            set => part = ModifyField(part, value);
        }

        public override void Clear()
        {
            field = default!;
            part = default;
        }

        public override string DefaultXmlElementName => XmlConsts.IconMapping;

        public override void Init() { }

        protected override void LoadData(XmlElement element)
        {
            part = XmlHelper.Value<IconContent>(element, XmlConsts.IconContentPart);
            field = XmlHelper.Value<TField>(element, XmlConsts.Field);
        }

        protected override void SaveData(XmlElement element, bool clearModified = true)
        {
            XmlHelper.AppendElement(element, XmlConsts.IconContentPart, part);
            XmlHelper.AppendElement(element, XmlConsts.Field, field);
        }

        public override string ToString() =>
            $"{TypeHelper.Name(part)}: {TypeHelper.Name(field)}";

        public override int CompareTo(DAO? other)
        {
            if (other is IconMapping<TField> otherMapping)
            {
                IconContent otherPart = otherMapping.Part;

                return part switch
                {
                    IconContent.Image => otherPart == IconContent.Image 
                        ? 0 : -1,
                    IconContent.Title => otherPart == IconContent.Image
                        ? 1 : otherPart == IconContent.Title ? 0 : -1,
                    IconContent.Left => otherPart == IconContent.Image || otherPart == IconContent.Title 
                        ? 1 : otherPart == IconContent.Left ? 0 : -1,
                    _ => //IconContent.Right
                        1
                };
            }

            return base.CompareTo(other);
        }
    }
}