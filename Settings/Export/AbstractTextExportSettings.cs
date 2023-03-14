using OxXMLEngine.Data.Types;
using OxXMLEngine.XML;
using System.Xml;
using OxXMLEngine.Data.Fields;

namespace OxXMLEngine.Settings
{
    public abstract class AbstractTextExportSettings<TField> : AbstractExportSettings
        where TField : notnull, Enum
    {
        public FieldColumns<TField> Fields { get; internal set; } = new()
        {
            XmlName = "Fields"
        };
        public ExportSummaryType Summary { get; set; }

        public bool IncludeExportParams { get; set; }

        public override void Clear()
        {
            base.Clear();
            Fields.Clear();
            Summary = TypeHelper.EmptyValue<ExportSummaryType>();
            IncludeExportParams = false;
        }

        public override void Init()
        {
            base.Init();
            Summary = TypeHelper.EmptyValue<ExportSummaryType>();
            IncludeExportParams = false;
        }

        protected override void LoadData(XmlElement element)
        {
            base.LoadData(element);
            Fields.Load(element);
            Summary = XmlHelper.Value<ExportSummaryType>(element, XmlConsts.SummaryType);
            IncludeExportParams = XmlHelper.ValueBool(element, XmlConsts.IncludeExportParams);
        }

        protected override void SaveData(XmlElement element, bool clearModified = true)
        {
            base.SaveData(element, clearModified);
            Fields.Save(element, clearModified);
            XmlHelper.AppendElement(element, XmlConsts.SummaryType, Summary);
            XmlHelper.AppendElement(element, XmlConsts.IncludeExportParams, IncludeExportParams);
        }
    }
}