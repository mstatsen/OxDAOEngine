using OxXMLEngine.Data;
using OxXMLEngine.XML;
using System.Xml;

namespace OxXMLEngine.Settings.Export
{
    public abstract class AbstractExportSettings : DAO
    {
        public string FileName { get; set; } = string.Empty;

        public override void Clear() =>
            FileName = string.Empty;

        public override void Init() =>
            FileName = string.Empty;

        protected override void LoadData(XmlElement element) =>
            FileName = XmlHelper.Value(element, XmlConsts.FileName);

        protected override void SaveData(XmlElement element, bool clearModified = true) =>
            XmlHelper.AppendElement(element, XmlConsts.FileName, FileName);
    }
}