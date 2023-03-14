using OxXMLEngine.Data;
using OxXMLEngine.Data.Filter;
using OxXMLEngine.Data.Types;
using OxXMLEngine.Export;
using OxXMLEngine.XML;
using System.Xml;

namespace OxXMLEngine.Settings
{
    public class ExportSettings<TField, TDAO> : DAO
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public ExportSettings() : base() { }

        public ExportHTMLSettings<TField, TDAO> HTML { get; internal set; } = new();
        public ExportXMLSettings XML { get; internal set; } = new();
        public ExportTextSettings<TField, TDAO> Text { get; internal set; } = new();

        public Filter<TField, TDAO> Filter { get; internal set; } = new();
        public ExportFormat Format { get; set; }

        public string? CategoryName { get; set; }

        public string FileName
        {
            get => GetFileName(Format);

            set
            {
                switch (Format)
                {
                    case ExportFormat.Html:
                        HTML.FileName = value;
                        break;
                    case ExportFormat.Xml:
                        XML.FileName = value;
                        break;
                    case ExportFormat.Text:
                        Text.FileName = value;
                        break;
                }
            }
        }

        public string GetFileName(ExportFormat format) => 
            format switch
            {
                ExportFormat.Html => HTML.FileName,
                ExportFormat.Xml => XML.FileName,
                ExportFormat.Text => Text.FileName,
                _ => string.Empty,
            };

        public override void Clear()
        {
            CategoryName = "All Items";
            Format = ExportFormat.Html;
            HTML.Clear();
            XML.Clear();
            Text.Clear();
            Filter.Clear();
        }

        public override void Init()
        {
            HTML.XmlName = TypeHelper.Name(ExportFormat.Html);
            XML.XmlName = TypeHelper.Name(ExportFormat.Xml);
            Text.XmlName = TypeHelper.Name(ExportFormat.Text);
            Format = ExportFormat.Html;
        }

        public Dictionary<string, string?> ParamsValues => new()
        {
            ["Category"] = CategoryName
        };

        public Dictionary<string, string> FilterValues
        {

            get
            {
                Dictionary<string, string> filterValues = new();
                SimpleFilter<TField, TDAO>? fieldsFilter = Filter?.Root[0][0][0];

                if (fieldsFilter != null)
                    foreach (FilterRule<TField, TDAO> rule in fieldsFilter.Rules)
                    {
                        TField field = rule.Field;
                        object? value = fieldsFilter[field];

                        if (value == null)
                            continue;

                        if (TypeHelper.IsTypeHelpered(value))
                            filterValues.Add(TypeHelper.Name(field), TypeHelper.Name(value));
                        else
                        {
                            string? stringValue = value.ToString();

                            if (stringValue != null)
                                filterValues.Add(TypeHelper.Name(field), stringValue);
                        }
                    }

                TextFilterOperationHelper helper = TypeHelper.Helper<TextFilterOperationHelper>();
                SimpleFilter<TField, TDAO>? textFilter = Filter?.Root[0][0][1];

                if (textFilter != null)
                    foreach (FilterRule<TField, TDAO> rule in textFilter.Rules)
                    {
                        TField field = rule.Field;
                        object? value = textFilter[field];

                        filterValues.Add(
                            TypeHelper.Name(field),
                            TypeHelper.IsTypeHelpered(value)
                                ? TypeHelper.Name(value)
                                : helper.DisplaySQLText(SettingsManager.DAOSettings<TField>().QuickFilterTextFieldOperation, value)
                        );
                    }

                return filterValues;
            }
        }

        protected override void LoadData(XmlElement element)
        {
            CategoryName = XmlHelper.Value(element, XmlConsts.CategoryName);
            Format = XmlHelper.Value<ExportFormat>(element, XmlConsts.ExportFormat);
            HTML.Load(element);
            XML.Load(element);
            Text.Load(element);
            Filter.Load(element);
        }

        protected override void SaveData(XmlElement element, bool clearModified = true)
        {
            XmlHelper.AppendElement(element, XmlConsts.CategoryName, CategoryName);
            XmlHelper.AppendElement(element, XmlConsts.ExportFormat, Format);
            Filter.Save(element, clearModified);

            HTML.Save(element, clearModified);
            XML.Save(element, clearModified);
            Text.Save(element, clearModified);
        }
    }
}
