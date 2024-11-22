using OxLibrary;
using OxDAOEngine.ControlFactory.Filter;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Filter;
using OxDAOEngine.ControlFactory;

namespace OxDAOEngine.Grid
{
    public class ItemSelector<TField, TDAO> : ItemsViewer<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public readonly QuickFilterPanel<TField, TDAO> QuickFilter = new(QuickFilterVariant.Select)
        {
            Visible = FunctionalPanelVisible.Fixed
        };

        public ItemSelector(RootListDAO<TField, TDAO>? itemList = null, GridUsage usage = GridUsage.SelectItem)
            : base(itemList, usage) { }

        public TDAO? SelectedItem
        {
            get => Grid.CurrentItem;
            set 
            {
                if (!DataManager.ListController<TField, TDAO>().AvailableQuickFilter)
                {
                    QuickFilter.SetFilter(value);
                    ApplyQuickFilter();
                }
            }
        }

        protected override void PrepareInnerControls()
        {
            base.PrepareInnerControls();

            if (DataManager.ListController<TField, TDAO>().AvailableQuickFilter)
                PrepareQuickFilter();
        }

        private void GridDoubleClickHandler(object? sender, EventArgs e)
        {
            if (PanelViewer is not null
                && SelectedItem is not null)
                PanelViewer.DialogResult = DialogResult.OK;
        }

        public override void Fill()
        {
            base.Fill();

            if (DataManager.ListController<TField, TDAO>().AvailableQuickFilter)
                ApplyQuickFilter();
        }

        private void PrepareQuickFilter()
        {
            if (!DataManager.ListController<TField, TDAO>().AvailableQuickFilter)
                return;

            QuickFilter.Parent = this;
            QuickFilter.Dock = OxDock.Top;
            QuickFilter.Margin.Bottom = OxWh.W4;
            QuickFilter.Height += 20;
            QuickFilter.Changed += (s, e) => ApplyQuickFilter();
            QuickFilter.RenewFilterControls();
        }

        private void ApplyQuickFilter()
        {
            if (DataManager.ListController<TField, TDAO>().AvailableQuickFilter)
                Grid?.ApplyQuickFilter(QuickFilter.ActiveFilter);
        }

        protected override void AfterCreated()
        {
            base.AfterCreated();
            Text = "Select Item";
        }
    }
}