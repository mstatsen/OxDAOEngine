using OxLibrary;
using OxXMLEngine.ControlFactory.Filter;
using OxXMLEngine.Data;
using OxXMLEngine.Data.Filter;

namespace OxXMLEngine.Grid
{
    public class ItemSelector<TField, TDAO> : ItemsViewer<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public readonly QuickFilterPanel<TField, TDAO> QuickFilterPanel = new(QuickFilterVariant.Select);

        public ItemSelector(RootListDAO<TField, TDAO>? itemList = null, GridUsage usage = GridUsage.SelectItem)
            : base(itemList, usage) => 
            ReAlign();

        public TDAO? SelectedItem
        {
            get => Grid.CurrentItem;
            set => QuickFilterPanel.SetFilter(value);
        }

        protected override void PrepareInnerControls()
        {
            base.PrepareInnerControls();
            PrepareQuickFilter();
        }

        private void GridDoubleClickHandler(object? sender, EventArgs e)
        {
            if (PanelViewer != null
                && SelectedItem != null)
            PanelViewer.DialogResult = DialogResult.OK;
        }

        public override void Fill()
        {
            base.Fill();
            ApplyQuickFilter();
        }

        private void PrepareQuickFilter()
        {
            QuickFilterPanel.Parent = ContentContainer;
            QuickFilterPanel.Dock = DockStyle.Top;
            QuickFilterPanel.Margins.BottomOx = OxSize.Large;
            QuickFilterPanel.Height += 20;
            QuickFilterPanel.Changed += QuickFilterChangedHandler;
            QuickFilterPanel.RenewFilterControls();
        }

        private void QuickFilterChangedHandler(object? sender, EventArgs e) =>
            ApplyQuickFilter();

        private void ApplyQuickFilter() =>
            Grid?.ApplyQuickFilter(QuickFilterPanel.ActiveFilter);

        protected override void AfterCreated()
        {
            base.AfterCreated();
            Text = "Select Item";
        }

        public override void ReAlignControls()
        {
            base.ReAlignControls();

            QuickFilterPanel.BringToFront();

            if (Grid != null)
                Grid.BringToFront();
        }
    }
}