using OxDAOEngine.Data;
using OxDAOEngine.Data.Types;
using OxDAOEngine.View.Types;
using OxDAOEngine.XML;
using System.Xml;

namespace OxDAOEngine.Settings.Data
{
    public class IconMapping<TField> : DAO
        where TField : notnull, Enum
    {
        private TField field = default!;

        public TField Field
        {
            get => @field;
            set => @field = EnumValue<TField>(ModifyValue(@field, value));
        }

        private IconContent part = default;

        public IconContent Part
        {
            get => part;
            set => part = ModifyValue(part, value);
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

        public override int CompareTo(IDAO? other)
        {
            if (other is IconMapping<TField> otherMapping)
            {
                IconContent otherPart = otherMapping.Part;

                return part switch
                {
                    IconContent.Image => 
                        otherPart is 
                            IconContent.Image 
                                ? 0 
                                : -1,
                    IconContent.Title => 
                        otherPart is IconContent.Image 
                            ? 1 
                            : otherPart is IconContent.Title ? 0 : -1,
                    IconContent.Left => 
                        otherPart is IconContent.Image 
                        || otherPart is IconContent.Title
                            ? 1 
                            : otherPart is IconContent.Left 
                                ? 0 
                                : -1,
                    _ => //IconContent.Right
                        1
                };
            }

            return base.CompareTo(other);
        }

        public override bool Equals(object? obj) => 
            base.Equals(obj)
            || (obj is IconMapping<TField> otherMapping
                && Part.Equals(otherMapping.Part)
                && Field.Equals(otherMapping.Field));

        public override int GetHashCode() => 
            Part.GetHashCode() ^ Field.GetHashCode();
    }
}