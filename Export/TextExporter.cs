using OxDAOEngine.Data;
using OxDAOEngine.Data.Decorator;
using OxDAOEngine.Data.Extract;
using OxDAOEngine.Data.Fields;
using OxDAOEngine.Data.Filter;
using OxDAOEngine.Data.Sorting;
using OxDAOEngine.Data.Types;
using OxDAOEngine.Settings;
using OxDAOEngine.Settings.Export;
using OxDAOEngine.XML;
using System.Text;

namespace OxDAOEngine.Export
{
    internal class TextExporter<TField, TDAO> : Exporter<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public TextExporter(ExportSettings<TField, TDAO> settings, ExportSettingsForm<TField, TDAO> settingsForm) 
            : base(settings, settingsForm)
        { }

        public override string Text()
        {
            StringBuilder builder = new();
            builder.Append(ExportParams());
            builder.Append(Summary());

            TDAO? prevItem = null;

            foreach (TDAO item in Items)
            {
                builder.Append(ItemGroups(prevItem, item));
                builder.Append(ItemLine(item));
                builder.AppendLine();
                prevItem = item;
            }

            return builder.ToString();
        }

        protected override FieldSortings<TField, TDAO> Sortings()
        {
            FieldSortings<TField, TDAO> sortings = new();
            sortings.CopyFrom(SettingsForm.groupByPanel.Sortings);

            FieldSortings<TField, TDAO>? defaultSorting = ListController.DefaultSorting();
            if (defaultSorting != null)
                sortings.AddRange(defaultSorting);

            foreach (FieldColumn<TField> column in Settings.Text.Fields)
                if (!Settings.Text.Grouping.Fields.Contains(column.Field))
                    sortings.Add(column.Field, SortOrder.Ascending);

            return sortings;
        }

        private string ItemLine(TDAO item)
        {
            StringBuilder builder = new(
                string.Join(string.Empty, Enumerable.Repeat(XmlConsts.DefaultIndent, Settings.Text.Grouping.Count))
            );

            Decorator<TField, TDAO> decorator = ListController.DecoratorFactory.Decorator(DecoratorType.FullInfo, item);

            builder.Append(decorator[TypeHelper.FieldHelper<TField>().TitleField]);
            builder.Append(InlineFields(decorator));
            return builder.ToString();
        }

        private string InlineFields(Decorator<TField, TDAO> decorator)
        {
            StringBuilder builder = new(string.Empty);
            string? fieldValue;

            foreach (FieldColumn<TField> column in Settings.Text.Fields)
            {
                fieldValue = decorator[column.Field]?.ToString()?.Trim();

                if (fieldValue == null ||
                    fieldValue == string.Empty)
                    continue;

                if (builder.Length > 0)
                    builder.Append(", ");

                builder.Append(fieldValue);
            }

            if (builder.Length > 0)
            {
                builder.Insert(0, " (");
                builder.Append(')');
            }

            return builder.ToString();
        }

        private string ItemGroups(TDAO? prevItem, TDAO item)
        {
            bool needStartGroup = false;
            int groupIndentCount = 0;
            string groupName;
            StringBuilder builder = new(string.Empty);
            Decorator<TField, TDAO> decorator = ListController.DecoratorFactory.Decorator(DecoratorType.FullInfo, item);
            FieldSortings<TField, TDAO> groupings = SettingsForm.groupByPanel.Sortings;

            foreach (FieldSorting<TField, TDAO> group in groupings)
            {
                if (needStartGroup || 
                    (prevItem == null) || 
                    prevItem[group.Field] == null || 
                    (!prevItem[group.Field]!.Equals(item[group.Field])))
                {
                    if (prevItem != null)
                    {
                        if (group == groupings.First())
                            builder.AppendLine();

                        if (!needStartGroup)
                            builder.AppendLine();
                    }

                    groupName = string.Join(string.Empty, Enumerable.Repeat(XmlConsts.DefaultIndent, groupIndentCount));
                    object? decorName = decorator[group.Field];
                    groupName += decorName != null ? decorName.ToString() : string.Empty;
                    builder.AppendLine(groupName.ToUpper());
                    builder.AppendLine();

                    needStartGroup = true;
                }

                groupIndentCount++;
            }

            return builder.ToString();
        }

        private string Summary()
        {
            if (Settings.Text.Summary == ExportSummaryType.None)
                return string.Empty;

            StringBuilder summaryBuilder = new(string.Empty);

            summaryBuilder.AppendLine(HardLineSeparator);
            summaryBuilder.AppendLine("Summary".ToUpper());
            summaryBuilder.AppendLine(HardLineSeparator);
            summaryBuilder.AppendFormat(ParamTemplate, ListController.Name, ItemsCount());
            summaryBuilder.AppendLine();
//TODO            AppendFieldInfo(summaryBuilder, GameField.Platform);
            summaryBuilder.AppendLine(HardLineSeparator);
            summaryBuilder.AppendLine();
            summaryBuilder.AppendLine();
            return summaryBuilder.ToString();
        }

        /*TODO
        private void AppendFieldInfo(StringBuilder builder, TField field)
        {
            FieldCountExtract extract = new FieldExtractor<TField, TDAO>(
                Settings.Text.Summary == ExportSummaryType.Exported
                    ? Items
                    : TotalItemsList).CountExtract(
                field,
                true
            );

            StringBuilder valueBuilder = new(string.Empty);

            foreach (var item in extract)
            {
                valueBuilder.Clear();

                if (Settings.Text.Summary == ExportSummaryType.Full)
                {
                    valueBuilder.Append(
                        Items.FilteredList(
                            new SimpleFilter<TField, TDAO>(field, FilterOperation.Equals, item.Key)
                        ).Count
                    );
                    valueBuilder.Append(" / ");
                }

                valueBuilder.Append(item.Value);
                builder.AppendFormat(ParamTemplate,
                    TypeHelper.Name(item.Key),
                    valueBuilder.ToString()
                );
                builder.AppendLine();
            }

        }
        */

        private string ItemsCount() => 
            Settings.Text.Summary == ExportSummaryType.Full 
                ? $"{Items.Count} / {TotalItemsList.Count}"
                : Items.Count.ToString();

        private RootListDAO<TField, TDAO> TotalItemsList => ListController.FullItemsList;

        private string ExportParams()
        {
            if (!Settings.Text.IncludeExportParams)
                return string.Empty;

            StringBuilder paramsBuilder = new(string.Empty);
            paramsBuilder.AppendLine(LineSeparator);
            paramsBuilder.AppendLine("Request".ToUpper());
            paramsBuilder.AppendLine(LineSeparator);

            foreach (var item in Settings.ParamsValues)
            {
                paramsBuilder.AppendFormat(ParamTemplate, item.Key, item.Value);
                paramsBuilder.AppendLine(string.Empty);
            }

            Dictionary<string, string> filterValues = Settings.FilterValues;

            if (filterValues.Count > 0)
            {
                paramsBuilder.AppendFormat(ParamTemplate, "Filter", string.Empty);
                paramsBuilder.AppendLine(string.Empty);
                bool textFilterExists = false;

                foreach (var item in filterValues)
                    if (item.Key == Consts.QuickFilterTextFieldCaption)
                        textFilterExists = true;
                    else
                    {
                        paramsBuilder.AppendFormat(FilterTemplate,
                            item.Key,
                            item.Value
                        );
                        paramsBuilder.AppendLine(string.Empty);
                    }


                if (textFilterExists)
                {
                    paramsBuilder.Append(XmlConsts.DefaultIndent);
                    paramsBuilder.Append(XmlConsts.DefaultIndent);
                    paramsBuilder.AppendFormat("One of item fields need contains '{0}'",
                        filterValues[Consts.QuickFilterTextFieldCaption]
                    );
                    paramsBuilder.AppendLine(string.Empty);
                }

                paramsBuilder.Remove(paramsBuilder.Length - 1, 1);
            }

            paramsBuilder.AppendLine(LineSeparator);
            paramsBuilder.AppendLine(string.Empty);
            paramsBuilder.AppendLine(string.Empty);
            return paramsBuilder.ToString();
        }

        private const string ParamTemplate = XmlConsts.DefaultIndent + "{0}: {1}";
        private const string FilterTemplate = XmlConsts.DefaultIndent + XmlConsts.DefaultIndent + "{0} = {1}";
        private const string LineSeparator = "--------------------------------------------------------------------------------";
        private const string HardLineSeparator = "________________________________________________________________________________";
    }
}
