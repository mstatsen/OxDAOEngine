using OxDAOEngine.XML;
using System.Xml;

namespace OxDAOEngine.Settings.Export
{
    public class ExportXMLSettings : AbstractExportSettings
    {
        public bool Indent { get; set; }

        public override void Clear()
        {
            base.Clear();
            Indent = true;
        }

        public override void Init()
        {
            base.Init();
            Indent = true;
        }

        protected override void LoadData(XmlElement element)
        {
            base.LoadData(element);
            Indent = XmlHelper.ValueBool(element, XmlConsts.Indent);
        }

        protected override void SaveData(XmlElement element, bool clearModified = true)
        {
            base.SaveData(element);
            XmlHelper.AppendElement(element, XmlConsts.Indent, Indent);
        }
    }
}