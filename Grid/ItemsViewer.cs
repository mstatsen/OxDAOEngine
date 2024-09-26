using OxLibrary;
using OxLibrary.Panels;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Filter;

namespace OxDAOEngine.Grid
{
    public class ItemsViewer<TField, TDAO> : OxPanel
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public readonly ItemsRootGrid<TField, TDAO> Grid;
        private IMatcher<TField>? filter;

        public IMatcher<TField>? Filter
        {
            get => filter;
            set
            {
                filter = value;
                Fill();
            }
        }

        public ItemsViewer(RootListDAO<TField, TDAO>? itemList = null, GridUsage usage = GridUsage.ViewItems)
            : base(new Size(1024, 768))
        {
            Grid = new ItemsRootGrid<TField, TDAO>(itemList, usage)
            {
                Parent = ContentContainer,
                Dock = DockStyle.Fill,
                GridContextMenuEnabled = false
            };
            Grid.Paddings.SetSize(OxSize.None);
            ReAlign();
        }

        public List<TField>? Fields
        {
            get => Grid.Fields;
            set => Grid.Fields = value;
        }

        public IRootListDAO<TField, TDAO>? CustomItemsList
        {
            get => Grid.CustomItemsList;
            set => Grid.CustomItemsList = value;
        }

        public List<CustomGridColumn<TField, TDAO>>? AdditionalColumns
        {
            get => Grid.AdditionalColumns;
            set => Grid.AdditionalColumns = value;
        }

        public virtual void Fill() =>
            Grid.Fill(Filter, true);

        protected override void AfterCreated()
        {
            base.AfterCreated();
            Text = "Select Item";
        }

        protected override void PrepareColors()
        {
            base.PrepareColors();

            if (Grid != null)
                Grid.BaseColor = Colors.Lighter(1);
        }

        protected override void PrepareDialog(OxPanelViewer dialog)
        {
            base.PrepareDialog(dialog);
            dialog.Sizeble = true;
            dialog.CanMaximize = true;
        }

        public bool SelectItem(Predicate<TDAO> match) => Grid.SelectItem(match);

        public bool SelectItem(TDAO? item) => Grid.SelectItem(item);

        public void ClearSelection() => Grid.GridView.ClearSelection();
        public void BeginUpdate() => Grid.BeginUpdate();
        public void EndUpdate() => Grid.EndUpdate();
        public IRootListDAO<TField, TDAO> ItemsList => Grid.ItemsList;

    }
}