using OxLibrary;
using OxLibrary.Panels;
using OxXMLEngine.Data;
using OxXMLEngine.Settings;

namespace OxXMLEngine.Summary
{
    public partial class SummaryView<TField, TDAO> : OxFrame
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public void Clear()
        {
            SummaryPanels.Clear();
            layouter.Clear();
        }

        private readonly OxPanelLayouter layouter = new();

        public void RefreshData(bool hardReset = false)
        {
            if (hardReset)
            {
                Clear();
                PrepareDictionaries();
            }

            IterateSummaryPanels((p) => p.FillAccessors());
            Update();
            IterateSummaryPanels((p) => p.AlignAccessors());
            IterateSummaryPanels((p) => p.CalcPanelSize());
            layouter.LayoutPanels(SummaryPanels);
            SummaryPanels.First().Expand();
        }

        public SummaryView() : base()
        {
            layouter.PanelsAlign = PanelsHorizontalAlign.OneColumn;
            Borders.HorizontalOx = OxSize.None;
            Borders.VerticalOx = OxSize.None;
        }

        public override Color DefaultColor => EngineStyles.SummaryColor;

        protected override void PrepareInnerControls()
        {
            base.PrepareInnerControls();
            PrepareDictionaries();
            ContentContainer.AutoScroll = true;
            layouter.Parent = ContentContainer;
            layouter.Dock = DockStyle.Top;
        }

        private void PrepareDictionaries()
        {
            List<ISummaryPanel>? generalSummaries = DataManager.ListController<TField, TDAO>().GeneralSummaries;
            if (generalSummaries != null)
                SummaryPanels.AddRange(generalSummaries);

            /*
                new GeneralSummary()
                {
                    Parent = ContentContainer,
                    Text = "General"
                }
            
            );
            */

            foreach (TField field in SettingsManager.DAOSettings<TField>().SummaryFields.Fields)
                SummaryPanels.Add(
                    new SummaryPanel<TField, TDAO>(field)
                );

            IterateSummaryPanels(
                (panel) =>
                    {
                        panel.Accordion = false;
                        panel.Expanded = false;
                        panel.BaseColor = EngineStyles.SummaryColor;
                        panel.Accordion = true;
                        panel.ExpandHandler += SummaryPanelExpandHandler;
                        panel.Header.UnderlineVisible = false;
                    }
            );

            SummaryPanels.Last().Header.UnderlineVisible = true;
        }

        private void SummaryPanelExpandHandler(object? sender, EventArgs e)
        {
            bool prevExpanded = false;

            IterateSummaryPanels(
                (panel) =>
                {
                    panel.Margins.Left = panel.Expanded ? 0 : 12;
                    panel.Margins.Right = panel.Expanded ? 0 : 12;
                    panel.Borders[OxDock.Top].Visible = !prevExpanded;
                    panel.Header.UnderlineVisible = panel.Expanded || panel == SummaryPanels.Last();
                    prevExpanded = panel.Expanded;
                }
            );

            RecalcSize();
        }

        private void IterateSummaryPanels(SummaryPanelHandler handler)
        {
            foreach (ISummaryPanel summaryPanel in SummaryPanels)
                handler(summaryPanel);
        }

        private readonly List<ISummaryPanel> SummaryPanels = new();

        protected override void PrepareColors()
        {
            base.PrepareColors();
            layouter.BaseColor = BaseColor;
        }
    }
}