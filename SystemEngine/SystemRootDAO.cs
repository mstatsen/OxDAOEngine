using OxXMLEngine.Data;
using System.Xml;

namespace OxXMLEngine.SystemEngine
{
    public class SystemRootDAO<TField> : RootDAO<TField>
        where TField : notnull, Enum
    {
        public override void Clear() { }
        public override void Init() { }
        protected override object? GetFieldValue(TField field) => null;
        protected override void LoadData(XmlElement element) { }
        protected override void SaveData(XmlElement element, bool clearModified = true) { }
        protected override void SetFieldValue(TField field, object? value) { }
    }
}
