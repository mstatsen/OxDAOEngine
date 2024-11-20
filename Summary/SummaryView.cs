﻿using OxLibrary;
using OxLibrary.Panels;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Types;
using OxDAOEngine.Data.Fields;

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
            layouter.PanelsAlign = PanelsHorizontalAlign.OneColumn;
            Borders.Size = OxWh.W0;
        }

        public override Color DefaultColor => EngineStyles.SummaryColor;

        protected override void PrepareInnerControls()
        {
            base.PrepareInnerControls();
            PrepareDictionaries();
            layouter.Parent = this;
            layouter.Dock = OxDock.Top;
            AutoScroll = true;
            //ContentBox.AutoScroll = true;
        }

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
                    panel.Margin.Horizontal = 
                        panel.Expanded 
                            ? OxWh.W12 
                            : OxWh.W32;
                    panel.Borders[OxDock.Top].Visible = !prevExpanded;
                    panel.Header.UnderlineVisible = 
                        panel.Expanded 
                        || panel.Equals(SummaryPanels.Last());
                    prevExpanded = panel.Expanded;
                }
            );

            RecalcSize();
        }

        private void IterateSummaryPanels(SummaryPanelHandler<TField, TDAO> handler)
        {
            foreach (SummaryPanel<TField, TDAO> summaryPanel in SummaryPanels)
                handler(summaryPanel);
        }

        private readonly List<SummaryPanel<TField, TDAO>> SummaryPanels = new();

        protected override void PrepareColors()
        {
            base.PrepareColors();
            layouter.BaseColor = BaseColor;
        }
    }
}