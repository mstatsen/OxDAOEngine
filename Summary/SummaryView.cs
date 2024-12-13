using OxLibrary.Panels;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Types;
using OxDAOEngine.Data.Fields;
using OxLibrary;
using OxLibrary.ControlList;
using OxLibrary.Geometry;

namespace OxDAOEngine.Summary
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

        public void RefreshData()
        {
            Clear();
            PrepareDictionaries();
            IterateSummaryPanels(p => p.FillAccessors());
            Update();
            IterateSummaryPanels(p => p.AlignAccessors());
            IterateSummaryPanels(p => p.CalcPanelSize());
            layouter.LayoutPanels(SummaryPanels);
            SummaryPanels.First().Expand();
        }

        public SummaryView() : base()
        {
            layouter.PanelsAlign = OxPanelsHorizontalAlign.OneColumn;
            Borders.Size = 0;
        }

        public override Color DefaultColor => EngineStyles.SummaryColor;

        protected override void PrepareInnerComponents()
        {
            base.PrepareInnerComponents();
            PrepareDictionaries();
            layouter.Parent = this;
            layouter.Dock = OxDock.Top;
            AutoScroll = true;
            //ContentBox.AutoScroll = true;
        }

        private readonly OxAccordionItems Accordion = new();

        private void PrepareDictionaries()
        {
            FieldHelper<TField> fieldHelper = TypeHelper.FieldHelper<TField>();
            foreach (TField field in fieldHelper.SummaryFields)
            {
                SummaryPanel<TField, TDAO> summaryPanel = new(field);
                summaryPanel.GetExtractItemCaption += DataManager.ListController<TField, TDAO>().GetExtractItemCaption;
                SummaryPanels.Add(summaryPanel);
            }

            SummaryPanel<TField, TDAO> generalSummaryPanel =
                new(DataManager.FieldHelper<TField>().FieldMetaData);
            generalSummaryPanel.GetExtractItemCaption += DataManager.ListController<TField, TDAO>().GetExtractItemCaption;
            SummaryPanels.Add(generalSummaryPanel);
            SummaryPanels.Reverse();

            IterateSummaryPanels(
                (panel) =>
                    {
                        panel.Expanded = OxB.F;
                        panel.BaseColor = EngineStyles.SummaryColor;
                        panel.ExpandChanged += SummaryPanelExpandHandler;
                        panel.Header.BorderVisible = OxB.F;
                        Accordion.Add(panel);
                    }
            );
            SummaryPanels.Last().BorderVisible = OxB.T;
        }

        private void SummaryPanelExpandHandler(object? sender, EventArgs e)
        {
            OxBool prevExpanded = OxB.F;

            IterateSummaryPanels(
                (panel) =>
                {
                    panel.Margin.Horizontal = OxSh.Short(panel.IsExpanded ? 12 : 32);
                    panel.Borders[OxDock.Top].Visible = OxB.Not(prevExpanded);
                    panel.SetBorderVisible(
                        panel.IsExpanded 
                        || panel.Equals(SummaryPanels.Last())
                    );
                    prevExpanded = panel.Expanded;
                }
            );

            //RecalcSize();
        }

        private void IterateSummaryPanels(SummaryPanelHandler<TField, TDAO> handler)
        {
            foreach (SummaryPanel<TField, TDAO> summaryPanel in SummaryPanels)
                handler(summaryPanel);
        }

        private readonly List<SummaryPanel<TField, TDAO>> SummaryPanels = new();

        public override void PrepareColors()
        {
            base.PrepareColors();
            layouter.BaseColor = BaseColor;
        }
    }
}