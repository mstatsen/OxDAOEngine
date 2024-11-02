using OxDAOEngine.Data;
using OxDAOEngine.Data.Sorting;
using System.Xml;

namespace OxDAOEngine.Settings.Export
{
    public class ExportTextSettings<TField, TDAO> : AbstractTextExportSettings<TField>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public FieldSortings<TField, TDAO> Grouping { get; internal set; } = new()
        {
            XmlName = "Grouping",
            SaveEmptyList = false
        };

        public override void Clear()
        {
            base.Clear();
            Grouping.Clear();
        }

        protected override void LoadData(XmlElement element)
        {
            base.LoadData(element);
            Grouping.Load(element);
        }

        protected override void SaveData(XmlElement element, bool clearModified = true)
        {
            base.SaveData(element, clearModified);
            Grouping.Save(element, clearModified);
        }
    }
}