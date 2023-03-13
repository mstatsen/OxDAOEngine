using OxLibrary;
using OxLibrary.Panels;
using OxXMLEngine.ControlFactory.Filter;
using OxXMLEngine.Data;
using OxXMLEngine.Data.Filter;

namespace OxXMLEngine.Grid
{
    public class ItemSelector<TField, TDAO> : OxPanel
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        private readonly ItemsGrid<TField, TDAO> grid;
        public readonly QuickFilterPanel<TField, TDAO> QuickFilterPanel = new(QuickFilterVariant.Select);
        private IMatcher<TDAO>? filter;

        public IMatcher<TDAO>? Filter 
        {
            get => filter;
            set
            {
                filter = value;
                Fill();
            }
        }

        public ItemSelector(RootListDAO<TField, TDAO>? itemList = null, GridUsage usage = GridUsage.SelectItem)
            : base(new Size(640, 480))
        {
            grid = new ItemsGrid<TField, TDAO>(itemList, usage)
            {
                Parent = ContentContainer,
                Dock = DockStyle.Fill
            };
            grid.DoubleClick += GridDoubleClickHandler;
            grid.Paddings.SetSize(OxSize.None);
            ReAlign();
        }

        public RootListDAO<TField, TDAO>? CustomItemsList
        {
            get => grid.CustomItemsList;
            set => grid.CustomItemsList = value;
        }

        public ItemsGrid<TField, TDAO> Grid => grid;

        public TDAO? SelectedItem
        {
            get => grid.CurrentItem;
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

        public void Fill()
        {
            grid.Fill(Filter, true);
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
            grid?.ApplyQuickFilter(QuickFilterPanel.ActiveFilter);

        protected override void AfterCreated()
        {
            base.AfterCreated();
            Text = "Select Item";
        }

        protected override void PrepareColors()
        {
            base.PrepareColors();

            if (grid != null)
              grid.BaseColor = Colors.Lighter(1);
        }

        public override void ReAlignControls()
        {
            base.ReAlignControls();

            QuickFilterPanel.BringToFront();

            if (grid != null)
                grid.BringToFront();
        }
    }
}