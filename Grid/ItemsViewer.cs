using OxLibrary;
using OxLibrary.Panels;
using OxXMLEngine.Data;
using OxXMLEngine.Data.Filter;

namespace OxXMLEngine.Grid
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
                Dock = DockStyle.Fill
            };
            Grid.Paddings.SetSize(OxSize.None);
            ReAlign();
        }

        public IRootListDAO<TField, TDAO>? CustomItemsList
        {
            get => Grid.CustomItemsList;
            set => Grid.CustomItemsList = value;
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
    }
}