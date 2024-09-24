using OxLibrary;
using OxLibrary.Controls;
using OxLibrary.Dialogs;
using OxLibrary.Panels;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Types;

namespace OxDAOEngine.Grid
{
    public class ItemsChooser<TField, TDAO> : OxPanel
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        private readonly ItemSelector<TField, TDAO> availableGrid = new(DataManager.FullItemsList<TField, TDAO>(), GridUsage.ChooseItems);
        private readonly ItemsRootGrid<TField, TDAO> selectedGrid = new(DataManager.FullItemsList<TField, TDAO>(), GridUsage.ChooseItems);
        private readonly OxPane buttonsPanel = new(new Size(64, 1));
        private readonly OxPane topPanel = new(new Size(1, 100));
        private readonly OxIconButton selectButton = new(OxIcons.Right, 54);
        private readonly OxIconButton unSelectButton = new(OxIcons.Left, 54);
        private readonly ItemsChooserParams<TField, TDAO> ChooserParams;
        private readonly OxFrameWithHeader availablePlace = new()
        { 
            Text = "Available Items"
        };
        private readonly OxFrameWithHeader selectedPlace = new()
        {
            Text = "Selected Items"
        };

        public ItemsChooser(
            ItemsChooserParams<TField, TDAO> chooserParams) : base(new Size(1280, 800))
        {
            ChooserParams = chooserParams;

            Text = ChooserParams.Title;
            BaseColor = ChooserParams.BaseColor;
            availablePlace.Text = ChooserParams.AvailableTitle;
            selectedPlace.Text = ChooserParams.SelectedTitle;

            availableGrid.Grid.Fields = chooserParams.AvailableGridFields;
            availableGrid.Grid.AdditionalColumns = chooserParams.AvailableGridAdditionalColumns;
            availableGrid.CustomItemsList = ChooserParams.AvailableItems;

            selectedGrid.CustomItemsList = new RootListDAO<TField, TDAO>();
            selectedGrid.Fields = chooserParams.SelectedGridFields;
            selectedGrid.AdditionalColumns = chooserParams.SelectedGridAdditionalColumns;

            if (DataManager.ListController<TField, TDAO>().AvailableQuickFilter)
                availableGrid.QuickFilter.ClearControls();

            availableGrid.Fill();
            PrepareSelectedGridItems(ChooserParams.InitialSelectedItems);
        }

        private void PrepareSelectedGridItems(RootListDAO<TField, TDAO> selectedItems)
        {
            availableGrid.Grid.GridView.ClearSelection();
            TField uniqueField = TypeHelper.FieldHelper<TField>().UniqueField;

            foreach (TDAO item in selectedItems)
            {
                object? itemUniqueValue = item[uniqueField];

                if (itemUniqueValue == null)
                    continue;

                TDAO? foundItem = availableGrid.Grid.ItemsList.Find(
                    g => itemUniqueValue.Equals(g[uniqueField]));

                if (foundItem == null)
                    continue;

                int rowIndex = availableGrid.Grid.GetRowIndex(foundItem);

                if (rowIndex > -1)
                    availableGrid.Grid.GridView.Rows[rowIndex].Selected = true;
            }

            MoveSelected(true, true);
        }

        protected override void PrepareColors()
        {
            base.PrepareColors();
            availableGrid.BaseColor = Colors.Darker(1);
            selectedGrid.BaseColor = BaseColor;
            buttonsPanel.BaseColor = BaseColor;
            availablePlace.BaseColor = BaseColor;
            selectedPlace.BaseColor = BaseColor;
            topPanel.BaseColor = BaseColor;
            /*selectButton.BaseColor = Colors.Darker();
            unSelectButton.BaseColor = Colors.Darker();
            */
            selectButton.BaseColor = BaseColor;
            unSelectButton.BaseColor = BaseColor;

        }

        protected override void PrepareInnerControls()
        {
            base.PrepareInnerControls();
            availablePlace.Parent = ContentContainer;
            availablePlace.Dock = DockStyle.Fill;
            availablePlace.Header.Underline.Visible = false;
            availableGrid.Parent = availablePlace;
            availableGrid.Dock = DockStyle.Fill;

            buttonsPanel.Parent = ContentContainer;
            buttonsPanel.Dock = DockStyle.Right;

            selectButton.Parent = buttonsPanel;
            selectButton.Click += (s, e) => MoveSelected(true);
            selectButton.Left = 4;
            selectButton.SetContentSize(54, 38);
            selectButton.HiddenBorder = false;

            unSelectButton.Parent = buttonsPanel;
            unSelectButton.Click += (s, e) => MoveSelected(false);
            unSelectButton.Left = 4;
            unSelectButton.SetContentSize(54, 38);
            unSelectButton.HiddenBorder = false;

            selectButton.Top = buttonsPanel.Height / 2 - selectButton.Height - 4;
            unSelectButton.Top = selectButton.Bottom + 8;

            selectedPlace.Parent = ContentContainer;
            selectedPlace.Dock = DockStyle.Right;
            selectedPlace.Header.Underline.Visible = false;
            selectedGrid.Parent = selectedPlace;
            selectedGrid.Dock = DockStyle.Fill;

            if (DataManager.ListController<TField, TDAO>().AvailableQuickFilter)
            {
                topPanel.Parent = ContentContainer;
                topPanel.Dock = DockStyle.Top;
                topPanel.SetContentSize(1,
                    availableGrid.QuickFilter.Height
                    + availableGrid.QuickFilter.Margins.Bottom
                );

                availableGrid.QuickFilter.Dock = DockStyle.Left;
                availableGrid.QuickFilter.Parent = topPanel;
                //availableGrid.QuickFilter.Margins.BottomOx = OxSize.Extra;
            }

            availableGrid.Grid.GridView.SelectionChanged += (s, e) => selectButton.Enabled = availableGrid.Grid.SelectedCount > 0;
            selectedGrid.GridView.SelectionChanged += (s, e) => unSelectButton.Enabled = selectedGrid.SelectedCount > 0;
        }

        private void MoveSelected(bool select, bool force = false)
        {
            ItemsRootGrid<TField, TDAO> sourceGrid = select ? availableGrid.Grid : selectedGrid;
            ItemsRootGrid<TField, TDAO> destGrid = select ? selectedGrid : availableGrid.Grid;

            bool? canSelect = true;

            RootListDAO<TField, TDAO> selectedList = sourceGrid.GetSelectedItems();
            destGrid.ItemsList.Modified = false;

            foreach (TDAO item in selectedList)
            {
                if (!force)
                    canSelect = select
                        ? ChooserParams.CanSelectItem?.Invoke(item, selectedList)
                        : ChooserParams.CanUnselectItem?.Invoke(item, selectedList);

                if (canSelect != null && canSelect == false)
                    return;

                destGrid.ItemsList.Add(item);
                sourceGrid.GridView.Rows.RemoveAt(sourceGrid.GetRowIndex(item));
                sourceGrid.ItemsList.Remove(item, false);
            }

            if (destGrid.ItemsList.Modified)
            {
                destGrid.ItemsList.Sort(DataManager.DefaultSorting<TField, TDAO>()?.SortingsList);
                destGrid.Fill();
            }

            if (select)
                ChooserParams.CompleteSelect?.Invoke(this, EventArgs.Empty);
            else ChooserParams.CompleteUnselect?.Invoke(this, EventArgs.Empty);
        }

        protected override void PrepareDialog(OxPanelViewer dialog)
        {
            base.PrepareDialog(dialog);
            dialog.Sizeble = true;
            dialog.CanMaximize = true;
            dialog.Shown += (s, e) => RecalcGridsSizes();
            dialog.SizeChanged += (s, e) => RecalcGridsSizes();
            availablePlace.SizeChanged += (s, e) => RecalcGridsSizes();

            if (DataManager.ListController<TField, TDAO>().AvailableQuickFilter)
                availableGrid.QuickFilter.Width = availablePlace.Width;
        }

        private void RecalcGridsSizes()
        {
            selectedPlace.Width = ((PanelViewer != null ? PanelViewer.Width : Width) - buttonsPanel.Width) / 2;
            selectButton.Top = buttonsPanel.Height / 2 - selectButton.Height - 50;
            unSelectButton.Top = selectButton.Bottom + 8;

            if (DataManager.ListController<TField, TDAO>().AvailableQuickFilter)
                availableGrid.QuickFilter.Width = availablePlace.Width;
        }

        public static bool ChooseItems(ItemsChooserParams<TField, TDAO> chooserParams, out RootListDAO<TField, TDAO> selection)
        {
            ItemsChooser<TField, TDAO> chooser = new(chooserParams);

            selection = new();
            bool result = chooser.ShowAsDialog(OxDialogButton.OK | OxDialogButton.Cancel) == DialogResult.OK;

            if (result)
                selection.LinkedCopyFrom(chooser.selectedGrid.ItemsList);

            return result;
        }
    }
}