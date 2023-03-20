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
        public readonly QuickFilterPanel<TField, TDAO> QuickFilter = new(QuickFilterVariant.Select)
        {
            IsSimplePanel = true
        };

        public ItemSelector(RootListDAO<TField, TDAO>? itemList = null, GridUsage usage = GridUsage.SelectItem)
            : base(itemList, usage) => 
            ReAlign();

        public TDAO? SelectedItem
        {
            get => Grid.CurrentItem;
            set => QuickFilter.SetFilter(value);
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
            QuickFilter.Parent = ContentContainer;
            QuickFilter.Dock = DockStyle.Top;
            QuickFilter.Margins.BottomOx = OxSize.Large;
            QuickFilter.Height += 20;
            QuickFilter.Changed += (s, e) => ApplyQuickFilter();
            QuickFilter.RenewFilterControls();
        }

        private void ApplyQuickFilter() =>
            Grid?.ApplyQuickFilter(QuickFilter.ActiveFilter);

        protected override void AfterCreated()
        {
            base.AfterCreated();
            Text = "Select Item";
        }

        public override void ReAlignControls()
        {
            base.ReAlignControls();

            QuickFilter.BringToFront();

            if (Grid != null)
                Grid.BringToFront();
        }
    }
}