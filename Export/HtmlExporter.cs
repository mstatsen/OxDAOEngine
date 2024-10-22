using OxDAOEngine.Data;
using OxDAOEngine.Data.Decorator;
using OxDAOEngine.Data.Extract;
using OxDAOEngine.Data.Filter;
using OxDAOEngine.Data.Fields;
using OxDAOEngine.Data.Sorting;
using OxDAOEngine.Data.Types;
using OxDAOEngine.Settings;
using System.Text;
using OxDAOEngine.View;
using OxDAOEngine.Settings.Export;
using OxDAOEngine.Data.Filter.Types;

namespace OxDAOEngine.Export
{
    internal class HtmlExporter<TField, TDAO> : Exporter<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public HtmlExporter(ExportSettings<TField, TDAO> settings, ExportSettingsForm<TField, TDAO> settingsForm) : 
            base(settings, settingsForm)
        { }

        public override string Text() =>
            string.Format(Templates.Html, HtmlExporter<TField, TDAO>.FullHead(), FullBody());

        protected override FieldSortings<TField, TDAO> Sortings() =>
            SettingsForm.htmlSortingPanel.Sortings;

        private static string FullHead() =>
            string.Format(Templates.Head, HtmlExporter<TField, TDAO>.HeadAttributes(), FullStyles());

        private static object HeadAttributes() =>
            string.Empty;

        private string FullBody() =>
            string.Format(Templates.Body,
                HtmlExporter<TField, TDAO>.BodyAttributes(),
                Request(),
                Summary(),
                FullTable()
            );

        private object Request() => 
            Settings.HTML.IncludeExportParams 
                ? string.Format(Templates.RequestCard, RequestBody())
                : string.Empty;

        private string RequestBody()
        {
            StringBuilder bodyBuilder = new(string.Empty);

            foreach (var item in Settings.ParamsValues)
                bodyBuilder.AppendLine(HtmlExporter<TField, TDAO>.RequestRow(item.Key, item.Value));

            Dictionary<string, string> filterValues = Settings.FilterValues;

            if (filterValues.Count > 0)
            {
                bodyBuilder.AppendLine("<br>");
                bodyBuilder.AppendLine(HtmlExporter<TField, TDAO>.RequestRow("Filter", string.Empty));

                foreach (var item in filterValues)
                    bodyBuilder.AppendLine(HtmlExporter<TField, TDAO>.FilterRow(item.Key, item.Value));
            }

            return bodyBuilder.ToString();
        }

        private static string RequestRow(string key, object? value)
        {
            string? stringValue = value?.ToString();
            stringValue = stringValue == null
                ? string.Empty 
                : stringValue.Replace(" ", "&nbsp;");

            return string.Format(Templates.RequestRow,
                key,
                stringValue
            );
        }

        private static string FilterRow(string key, object value)
        {
            string? stringValue = value.ToString();
            stringValue = stringValue == null ? string.Empty : stringValue.Replace(" ", "&nbsp;");
            return string.Format(Templates.FilterRow,
                key,
                stringValue
            );
        }

        private object Summary()
        {
            if (Settings.HTML.Summary == ExportSummaryType.None)
                return string.Empty;

            StringBuilder summaryBuilder = new(string.Empty);
            summaryBuilder.AppendLine(SummaryCard("Summary", GeneralSummary()));

            /*
            if (Items.Count > 0)
            {
                summaryBuilder.AppendLine(ByFieldSummaryCard(GameField.Platform));
                summaryBuilder.AppendLine(ByFieldSummaryCard(GameField.Source));
                summaryBuilder.AppendLine(ByFieldSummaryCard(GameField.Status));
                summaryBuilder.AppendLine(TrophiesCard());
            }
            */

            return summaryBuilder.ToString();
        }

        private string TrophiesCard() =>
            SummaryCard(
                "Trophies",
                TrophiesSummary()
            );

        private static string TrophiesSummary()
        {
            StringBuilder bodyBuilder = new(string.Empty);

            /*
            PSNLevelCalculator exportLevel = new PSNLevelCalculator(Games);
            PSNLevelCalculator totalLevel = new PSNLevelCalculator(GamesController.FullItemsList);
            LevelValueTypeHelper helper = TypeHelper.Helper<LevelValueTypeHelper>();

            foreach (LevelValueType valueType in TypeHelper.All<LevelValueType>())
                if (helper.Group(valueType) == LevelValueTypeGroup.Trophies)
                    bodyBuilder.AppendLine(
                        SummaryRow(
                            TypeHelper.Name(valueType),
                            int.Parse(exportLevel.Value(valueType)),
                            int.Parse(totalLevel.Value(valueType))
                        )
                    );
            */

            return bodyBuilder.ToString();
        }

        private string ByFieldSummaryCard(TField field) =>
            SummaryCard(
                $"By {TypeHelper.Name(field).ToLower()}", 
                ByFieldSummary(field)
            );

        private string ByFieldSummary(TField field)
        {
            StringBuilder bodyBuilder = new(string.Empty);

            FieldCountExtract extract = new FieldExtractor<TField, TDAO>(
                ListController.FullItemsList
            ).CountExtract(field, true);

            foreach (var item in extract)
                bodyBuilder.AppendLine(
                    SummaryRow(
                        TypeHelper.Name(item.Key),
                        ListController.FullItemsList
                            .FilteredList(new SimpleFilter<TField, TDAO>(field, FilterOperation.Equals, item.Key)).Count,
                        item.Value
                    )
                );

            return bodyBuilder.ToString();
        }

        private string GeneralSummary()
        {
            if (Items.Count == 0)
                return "<i>No data to display</i>";

            StringBuilder bodyBuilder = new(string.Empty);
            bodyBuilder.AppendLine(
                SummaryRow(ListController.ListName, Items.Count, 
                    ListController.TotalCount)

            );
            /*
            bodyBuilder.AppendLine(
                SummaryRow("Trophysets",
                    GamesController.DistinctTrophysets(Games).Count, 
                    GamesController.DistinctTrophysets(GamesController.FullItemsList).Count
                )
            );
            bodyBuilder.AppendLine(
                SummaryRow("Unverified",
                    Games.FilteredList(new SimpleFilter<GameField, Game>(GameField.Verified, FilterOperation.Equals, false)).Count,
                    GamesController.FullItemsList.FilteredList(new SimpleFilter<GameField, Game>(GameField.Verified, FilterOperation.Equals, false)).Count)
            );
            */

            return bodyBuilder.ToString();
        }

        private string SummaryCard(string caption, string body) =>
            string.Format(Templates.SummaryCard,
                SummaryCardClass,
                caption,
                body
            );

        private string SummaryRow(string key, int value, int totalValue) =>
            Settings.HTML.ZeroSummary || (value != 0) 
                ? string.Format(Templates.SummaryRow,
                    key,
                    value,
                    Settings.HTML.Summary == ExportSummaryType.Full
                        ? string.Format(Templates.TotalValue, totalValue)
                        : string.Empty)
                : string.Empty;

        private string SummaryCardClass =>
            Settings.HTML.Summary == ExportSummaryType.Full
                ? "cardTotal"
                : "card";

        private static object BodyAttributes() =>
            string.Empty;

        private string FullTable() =>
            string.Format(Templates.Table,
                HtmlExporter<TField, TDAO>.TableAttributes(), 
                TableHeader(), 
                TableBody()
            );

        private static string TableAttributes() =>
            string.Empty;

        private string TableHeader()
        {
            StringBuilder rowBuilder = new();

            foreach (FieldColumn<TField> column in Settings.HTML.Fields)
                rowBuilder.AppendFormat(
                    Templates.HeaderCell,
                    HtmlExporter<TField, TDAO>.TableHeaderAttributes(),
                    TypeHelper.FieldHelper<TField>().ColumnCaption(column.Field));

            return string.Format(Templates.HeaderRow, HtmlExporter<TField, TDAO>.TableHeaderAttributes(), rowBuilder.ToString());
        }

        private static string TableHeaderAttributes() =>
            string.Empty;

        private string TableBody()
        {
            StringBuilder builder = new(string.Empty);

            foreach (TDAO item in Items)
                builder.Append(ItemRow(item));

            return builder.ToString();
        }

        private string ItemRow(TDAO item)
        {
            Decorator<TField, TDAO> decorator = ListController.DecoratorFactory.Decorator(DecoratorType.Html, item);
            StringBuilder rowBuilder = new(string.Empty);

            foreach (FieldColumn<TField> column in Settings.HTML.Fields)
            {
                rowBuilder.Append(
                    string.Format(Templates.Cell,
                        decorator.Attributes(column.Field),
                        decorator[column.Field]
                    )
                );
            }

            return string.Format(Templates.Row, 
                TableRowAttributes(item), 
                rowBuilder.ToString()
            );
        }

        private static object TableRowAttributes(TDAO item)
        {
            ItemColorer<TField, TDAO> colorer = DataManager.ControlFactory<TField, TDAO>().ItemColorer;
            string fontColor = ColorTranslator.ToHtml(colorer.ForeColor(item));
            string backColor = ColorTranslator.ToHtml(colorer.BackColor(item));
            return $" style =\"color: {fontColor}; background-color: {backColor};\"";
        }

        private static string FullStyles()
        {
            List<string?> styles = new()
            {
                "table {border: 1px solid silver; border-collapse: collapse;}",
                "thead {position: sticky; top: 0; z-index: 2;}",
                "th {vertical-align: middle; color: #4f5f7f; background-color: #e6e6e6;}",
                "td, th {border: 1px solid silver; padding-left: 8px; padding-right: 8px;}",
                "body {background-color: #f8faff}",
                ".request {width: 300px; background-color: #f8f8f8; border: 1px solid silver; margin-bottom: 12px; margin-right: 12px; display: inline-block; vertical-align: top;}",
                ".requestHead {width: inherit; height: 30; background-color: #f0f0f0; border-bottom: 1px solid silver; color: #4f4f28; font-weight: bold; text-align: center; display: table-cell; vertical-align: middle;}",
                ".card {width: 180px; background-color: #ffffd8; border: 1px solid silver; margin-bottom: 12px; margin-right: 12px; display: inline-block; vertical-align: top;}",
                ".cardTotal {width: 280px; background-color: #ffffd8; border: 1px solid silver; margin-bottom: 12px; margin-right: 12px; display: inline-block; vertical-align: top;}",
                ".cardHead {width: inherit; height: 30; background-color: #f8f8d0; border-bottom: 1px solid silver; color: #4f4f28; font-weight: bold; text-align: center; display: table-cell; vertical-align: middle;}",
                ".cardBody { padding: 12px 12px 12px 12px;}",
                ".key { display: inline-block; width: 120px;}",
                ".paramKey { display: inline-block; width: 140px; font-style: italic;}",
                ".filterKey { display: inline-block; width: 120px; font-style: italic;}",
                ".value { display: inline-block; width: 30px;}",
                ".total { display: inline-block; font-style: italic; margin-left: 16px;}",
                ".space { display: inline-block; font-style: italic; margin-left: 16px;}"
            };

            return string.Format(Templates.Styles, DecoratorHelper.ListToString(styles, "\n      ", false));
        }

        private static class Templates
        {
            public const string Html = "<html>{0}{1}</html>";
            public const string Head = "\n  <head{0}>{1}\n  </head>";
            public const string Styles = "\n    <style type=\"text/css\">\n{0}\n    </style>";
            public const string Body = "\n  <body{0}>{1}{2}{3}\n  </body>\n";
            public const string Table = 
                "\n    <table{0}>"+
                "\n      <thead>{1}"+
                "\n      </thead>"+
                "\n      <tbody>{2}"+
                "\n      </tbody>"+
                "\n    </table>";
            public const string Row = "\n        <tr{0}>{1}\n        </tr>";
            public const string Cell = "\n          <td{0}>{1}</td>";
            public const string HeaderRow = "\n        <tr{0}>{1}\n        </tr>";
            public const string HeaderCell = "\n          <th{0}>{1}</th>";
            public const string SummaryCard =
                "\n<div class=\"{0}\">" +
                "\n  <div class=\"cardHead\">" +
                "\n    {1}" +
                "\n  </div>" +
                "\n  <div class=\"cardBody\">" +
                "\n    {2}"+
                "\n  </div>" +
                "\n</div>";
            public const string SummaryRow = "<div class=\"key\">{0}:</div><div class=\"value\">{1}</div>{2}<br>";
            public const string TotalValue = "<div class=\"total\">(total: {0})</div>";
            public const string RequestCard =
                "\n<div class=\"request\">" +
                "\n  <div class=\"requestHead\">Request</div>" +
                "\n  <div class=\"cardBody\">" +
                "\n    {0}" +
                "\n  </div>" +
                "\n</div>";
            public const string RequestRow = "<div class=\"paramKey\">{0}:</div><div class=\"value\">{1}</div><br>";
            public const string Space = "<div class=\"space\">&nbsp;</div>";
            public const string FilterRow = Space + "<div class=\"filterKey\">{0}:</div><div class=\"value\">{1}</div><br>";
        }
    }
}