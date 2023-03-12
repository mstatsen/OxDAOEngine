using OxXMLEngine.Data.Types;
using OxXMLEngine.XML;
using System.Xml;

namespace OxXMLEngine.Data.Fields
{
    public class FieldColumn<TField> : DAO
        where TField: notnull, Enum
    {
        public TField Field { get; set; } = default!;

        public override void Clear() =>
            Field = TypeHelper.DefaultValue<TField>();

        protected override void SaveData(XmlElement element, bool clearModified = true) =>
            XmlHelper.AppendElement(element, XmlConsts.Field, Field);

        protected override void LoadData(XmlElement element) =>
            Field = TypeHelper.Parse<TField>(element.InnerText);

        public override void Init() => 
            Clear();

        public override string DefaultXmlElementName =>
            XmlConsts.Column;

        public override string? ToString() =>
            TypeHelper.Name(Field);

        public override bool Equals(object? obj) =>
            obj is FieldColumn<TField> column
                && (base.Equals(obj) ||
                    (Field.Equals(column.Field)));

        public override int GetHashCode() =>
            2049151602 + Field.GetHashCode();


        public FieldColumn() =>
            WithoutXmlNode = true;

        public FieldColumn(TField field) : this() =>
           
            Field = field;
    }
}