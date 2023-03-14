using OxXMLEngine.Data;
using OxXMLEngine.Data.Decorator;
using OxXMLEngine.Data.Extract;
using OxXMLEngine.Data.Fields;
using OxXMLEngine.Data.Filter;
using OxXMLEngine.Data.Sorting;
using OxXMLEngine.Data.Types;
using OxXMLEngine.Settings;
using OxXMLEngine.XML;
using System.Text;

namespace OxXMLEngine.Export
{
    internal class TextExporter<TField, TDAO> : Exporter<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public TextExporter(ExportSettings<TField, TDAO> settings) : base(settings)
        { }

        public override string Text()
        {
            StringBuilder builder = new();
            builder.Append(ExportParams());
            builder.Append(Summary());

            TDAO? prevGame = null;

            foreach (TDAO game in ListController.FullItemsList)
            {
                if (prevGame != null)
                    builder.Append(GameGroups(prevGame, game));

                builder.Append(GameLine(game));
                builder.AppendLine();
                prevGame = game;
            }

            return builder.ToString();
        }

        protected override FieldSortings<TField, TDAO> Sortings()
        {
            FieldSortings<TField, TDAO> sortings = new();
            sortings.CopyFrom(Settings.Text.Grouping);

            FieldSortings<TField, TDAO>? defaultSorting = ListController.DefaultSorting();
            if (defaultSorting != null)
                sortings.AddRange(defaultSorting);

            foreach (FieldColumn<TField> column in Settings.Text.Fields)
                if (!Settings.Text.Grouping.Fields.Contains(column.Field))
                    sortings.Add(column.Field, SortOrder.Ascending);

            return sortings;
        }

        private string GameLine(TDAO game)
        {
            StringBuilder builder = new(
                string.Join("", Enumerable.Repeat(XmlConsts.DefaultIndent, Settings.Text.Grouping.Count))
            );

            Decorator<TField, TDAO> decorator = ListController.DecoratorFactory.Decorator(DecoratorType.FullInfo, game);

            //builder.Append(decorator[GameField.Name]);
            builder.Append(InlineFields(decorator));
            return builder.ToString();
        }

        private string InlineFields(Decorator<TField, TDAO> decorator)
        {
            StringBuilder builder = new("");
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

        private string GameGroups(TDAO prevGame, TDAO game)
        {
            bool needStartGroup = false;
            int groupIndentCount = 0;
            string groupName;
            StringBuilder builder = new("");
            Decorator<TField, TDAO> decorator = ListController.DecoratorFactory.Decorator(DecoratorType.FullInfo, game);

            foreach (FieldSorting<TField, TDAO> group in Settings.Text.Grouping)
            {
                if (needStartGroup || 
                    (prevGame == null) || 
                    prevGame[group.Field] != null || 
                    (!prevGame[group.Field]!.Equals(game[group.Field])))
                {
                    if (prevGame != null)
                    {
                        if (group == Settings.Text.Grouping.First())
                            builder.AppendLine();

                        if (!needStartGroup)
                            builder.AppendLine();
                    }

                    groupName = string.Join("", Enumerable.Repeat(XmlConsts.DefaultIndent, groupIndentCount));
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

            StringBuilder summaryBuilder = new("");

            summaryBuilder.AppendLine(HardLineSeparator);
            summaryBuilder.AppendLine("Summary".ToUpper());
            summaryBuilder.AppendLine(HardLineSeparator);
            summaryBuilder.AppendFormat(ParamTemplate, "Games", GamesCount());
            summaryBuilder.AppendLine();
//TODO            AppendFieldInfo(summaryBuilder, GameField.Platform);
            summaryBuilder.AppendLine(HardLineSeparator);
            summaryBuilder.AppendLine();
            summaryBuilder.AppendLine();
            return summaryBuilder.ToString();
        }

        private void AppendFieldInfo(StringBuilder builder, TField field)
        {
            FieldCountExtract extract = new FieldExtractor<TField, TDAO>(
                Settings.Text.Summary == ExportSummaryType.Exported
                    ? Items
                    : TotalGamesList).CountExtract(
                field,
                true
            );

            StringBuilder valueBuilder = new("");

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

        private string GamesCount() => 
            Settings.Text.Summary == ExportSummaryType.Full 
                ? $"{Items.Count} / {TotalGamesList.Count}"
                : Items.Count.ToString();

        private RootListDAO<TField, TDAO> TotalGamesList => ListController.FullItemsList;

        private string ExportParams()
        {
            if (!Settings.Text.IncludeExportParams)
                return string.Empty;

            StringBuilder paramsBuilder = new("");
            paramsBuilder.AppendLine(LineSeparator);
            paramsBuilder.AppendLine("Request".ToUpper());
            paramsBuilder.AppendLine(LineSeparator);

            foreach (var item in Settings.ParamsValues)
            {
                paramsBuilder.AppendFormat(ParamTemplate, item.Key, item.Value);
                paramsBuilder.AppendLine("");
            }

            Dictionary<string, string> filterValues = Settings.FilterValues;

            if (filterValues.Count > 0)
            {
                paramsBuilder.AppendFormat(ParamTemplate, "Filter", "");
                paramsBuilder.AppendLine("");
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
                        paramsBuilder.AppendLine("");
                    }


                if (textFilterExists)
                {
                    paramsBuilder.Append(XmlConsts.DefaultIndent);
                    paramsBuilder.Append(XmlConsts.DefaultIndent);
                    paramsBuilder.AppendFormat("One of game fields need contains '{0}'",
                        filterValues[Consts.QuickFilterTextFieldCaption]
                    );
                    paramsBuilder.AppendLine("");
                }

                paramsBuilder.Remove(paramsBuilder.Length - 1, 1);
            }

            paramsBuilder.AppendLine(LineSeparator);
            paramsBuilder.AppendLine("");
            paramsBuilder.AppendLine("");
            return paramsBuilder.ToString();
        }

        private const string ParamTemplate = XmlConsts.DefaultIndent + "{0}: {1}";
        private const string FilterTemplate = XmlConsts.DefaultIndent + XmlConsts.DefaultIndent + "{0} = {1}";
        private const string LineSeparator = "--------------------------------------------------------------------------------";
        private const string HardLineSeparator = "________________________________________________________________________________";
    }
}
