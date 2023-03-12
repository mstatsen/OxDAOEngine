using OxLibrary;
using OxLibrary.Controls;
using OxLibrary.Dialogs;
using OxLibrary.Panels;
using OxXMLEngine.Data;
using OxXMLEngine.Data.Types;

namespace OxXMLEngine.Grid
{
    public class ItemsChooser<TField, TDAO> : OxPanel
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        private readonly ItemSelector<TField, TDAO> availableGrid = new(DataManager.FullItemsList<TField, TDAO>(), GridUsage.ChooseItems);
        private readonly ItemsGrid<TField, TDAO> selectedGrid = new(DataManager.FullItemsList<TField, TDAO>(), GridUsage.ChooseItems);
        private readonly OxPane buttonsPanel = new(new Size(64, 1));
        private readonly OxPane topPanel = new(new Size(1, 100));
        private readonly OxIconButton selectButton = new(OxIcons.right, 54);
        private readonly OxIconButton unSelectButton = new(OxIcons.left, 54);
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

            selectedGrid.CustomItemsList = new();
            selectedGrid.Fields = chooserParams.SelectedGridFields;
            selectedGrid.AdditionalColumns = chooserParams.SelectedGridAdditionalColumns;

            availableGrid.QuickFilterPanel.ClearControls();
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

                TDAO? foundItem = availableGrid.Grid.ItemsList.Find((g) =>
                    itemUniqueValue.Equals(g[uniqueField]));

                if (foundItem == null)
                    return;

                int rowIndex = availableGrid.Grid.GetRowIndex(foundItem);

                if (rowIndex > -1)
                    availableGrid.Grid.GridView.Rows[rowIndex].Selected = true;
            }

            MoveSelected(true, true);
        }

        protected override void PrepareColors()
        {
            base.PrepareColors();
            availableGrid.BaseColor = BaseColor;
            selectedGrid.BaseColor = Colors.Lighter(1);
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
            selectButton.Click += SelectHanlder;
            selectButton.Left = 4;
            selectButton.SetContentSize(54, 38);
            selectButton.HiddenBorder = false;

            unSelectButton.Parent = buttonsPanel;
            unSelectButton.Click += UnselectHanlder;
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

            topPanel.Parent = ContentContainer;
            topPanel.Dock = DockStyle.Top;
            topPanel.SetContentSize(1, 
                availableGrid.QuickFilterPanel.Height 
                + availableGrid.QuickFilterPanel.Margins.Bottom
            );

            availableGrid.QuickFilterPanel.Dock = DockStyle.Left;
            availableGrid.QuickFilterPanel.Parent = topPanel;
            availableGrid.QuickFilterPanel.Margins.BottomOx = OxSize.Extra;

            availableGrid.Grid.GridView.SelectionChanged += AvailableGridSelectionChanger;
            selectedGrid.GridView.SelectionChanged += SelectedGridSelectionChanger;
        }

        private void AvailableGridSelectionChanger(object? sender, EventArgs e) => 
            selectButton.Enabled = availableGrid.Grid.SelectedCount > 0;

        private void SelectedGridSelectionChanger(object? sender, EventArgs e) =>
            unSelectButton.Enabled = selectedGrid.SelectedCount > 0;

        private void MoveSelected(bool select, bool force = false)
        {
            ItemsGrid<TField, TDAO> sourceGrid = select ? availableGrid.Grid : selectedGrid;
            ItemsGrid< TField, TDAO > destGrid = select ? selectedGrid : availableGrid.Grid;

            DialogResult? canSelect = DialogResult.OK;
            bool forAll = false;

            RootListDAO<TField, TDAO> selectedList = sourceGrid.GetSelectedItems();
            destGrid.ItemsList.Modified = false;

            foreach (TDAO item in selectedList)
            {
                if (!force && !forAll)
                    canSelect = select
                        ? ChooserParams.CanSelectItem?.Invoke(item, selectedList)
                        : ChooserParams.CanUnselectItem?.Invoke(item, selectedList);


                if (canSelect != null)
                    switch (canSelect)
                    {
                        case DialogResult.Cancel:
                            return;
                        case DialogResult.Continue:
                            forAll = true;
                            break;
                    }

                destGrid.ItemsList.Add(item);
                sourceGrid.GridView.Rows.RemoveAt(sourceGrid.GetRowIndex(item));
                sourceGrid.ItemsList.Remove(item);
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

        private void UnselectHanlder(object? sender, EventArgs e) =>
            MoveSelected(false);

        private void SelectHanlder(object? sender, EventArgs e) =>
            MoveSelected(true);

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            selectedPlace.Width = (Width - buttonsPanel.Width) / 2;
            selectButton.Top = buttonsPanel.Height / 2 - selectButton.Height - 50;
            unSelectButton.Top = selectButton.Bottom + 8;
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