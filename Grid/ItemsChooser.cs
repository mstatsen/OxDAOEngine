using OxLibrary;
using OxLibrary.Controls;
using OxLibrary.Panels;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Types;
using OxLibrary.Forms;
using OxLibrary.Geometry;

namespace OxDAOEngine.Grid
{
    public class ItemsChooser<TField, TDAO> : OxPanel
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        private readonly ItemSelector<TField, TDAO> availableGrid = new(DataManager.FullItemsList<TField, TDAO>(), GridUsage.ChooseItems);
        private readonly ItemsRootGrid<TField, TDAO> selectedGrid = new(DataManager.FullItemsList<TField, TDAO>(), GridUsage.ChooseItems);
        private readonly OxPanel buttonsPanel = new(new(64, 1));
        private readonly OxPanel topPanel = new(new(1, 100));
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

        protected override Bitmap? GetIcon() => OxIcons.Replace;

        public ItemsChooser(
            ItemsChooserParams<TField, TDAO> chooserParams) : base(new(1280, 800))
        {
            ChooserParams = chooserParams;

            Text = ChooserParams.Title;
            BaseColor = ChooserParams.BaseColor;
            availablePlace.Text = ChooserParams.AvailableTitle;
            selectedPlace.Text = ChooserParams.SelectedTitle;
            selectButton.ToolTipText = ChooserParams.SelectButtonTip;
            unSelectButton.ToolTipText = ChooserParams.UnselectButtonTip;

            availableGrid.Fields = chooserParams.AvailableGridFields;
            availableGrid.AdditionalColumns = chooserParams.AvailableGridAdditionalColumns;
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
            availableGrid.BeginUpdate();

            try
            {
                availableGrid.ClearSelection();
                TField uniqueField = TypeHelper.FieldHelper<TField>().UniqueField;

                foreach (TDAO item in selectedItems)
                {
                    object? itemUniqueValue = item[uniqueField];

                    if (itemUniqueValue is null)
                        continue;

                    availableGrid.SelectItem(g => itemUniqueValue.Equals(g[uniqueField]));
                }

                MoveSelected(true, true);
            }
            finally
            {
                availableGrid.EndUpdate();
            }
        }

        public override void PrepareColors()
        {
            base.PrepareColors();
            availableGrid.BaseColor = Colors.Darker();
            selectedGrid.BaseColor = BaseColor;
            buttonsPanel.BaseColor = BaseColor;
            availablePlace.BaseColor = BaseColor;
            selectedPlace.BaseColor = BaseColor;
            topPanel.BaseColor = BaseColor;
            selectButton.BaseColor = BaseColor;
            unSelectButton.BaseColor = BaseColor;

        }

        protected override void PrepareInnerComponents()
        {
            base.PrepareInnerComponents();
            availablePlace.Parent = this;
            availablePlace.Dock = OxDock.Fill;
            availablePlace.Header.Underline.Visible = false;
            availableGrid.Parent = availablePlace;
            availableGrid.Dock = OxDock.Fill;

            buttonsPanel.Parent = this;
            buttonsPanel.Dock = OxDock.Right;

            selectButton.Parent = buttonsPanel;
            selectButton.Click += (s, e) => MoveSelected(true);
            selectButton.Left = 4;
            selectButton.Size = new(54, 38);
            selectButton.HiddenBorder = false;

            unSelectButton.Parent = buttonsPanel;
            unSelectButton.Click += (s, e) => MoveSelected(false);
            unSelectButton.Left = 4;
            unSelectButton.Size = new(54, 38);
            unSelectButton.HiddenBorder = false;

            selectButton.Top = 
                OxSH.Sub(
                    OxSH.Half(buttonsPanel.Height),
                    selectButton.Height, 
                    4
                );
            unSelectButton.Top = OxSH.Add(selectButton.Bottom, 8);

            selectedPlace.Parent = this;
            selectedPlace.Dock = OxDock.Right;
            selectedPlace.Header.Underline.Visible = false;
            selectedGrid.Parent = selectedPlace;
            selectedGrid.Dock = OxDock.Fill;

            if (DataManager.ListController<TField, TDAO>().AvailableQuickFilter)
            {
                topPanel.Parent = this;
                topPanel.Dock = OxDock.Top;
                topPanel.Size = new
                (
                    1,
                    availableGrid.QuickFilter.Height
                    + availableGrid.QuickFilter.Margin.Bottom
                );

                availableGrid.QuickFilter.Dock = OxDock.Left;
                availableGrid.QuickFilter.Parent = topPanel;
            }

            availableGrid.Grid.CurrentItemChanged += AvailableGridCurrentItemChanged;
            selectedGrid.CurrentItemChanged += SelectedGridCurrentItemChanged;
        }

        private void SelectedGridCurrentItemChanged(object? sender, EventArgs e) => 
            unSelectButton.Enabled = selectedGrid.SelectedCount > 0;

        private void AvailableGridCurrentItemChanged(object? sender, EventArgs e) => 
            selectButton.Enabled = availableGrid.Grid.SelectedCount > 0;

        private void MoveSelected(bool select, bool force = false)
        {
            ItemsRootGrid<TField, TDAO> sourceGrid = select ? availableGrid.Grid : selectedGrid;
            ItemsRootGrid<TField, TDAO> destGrid = select ? selectedGrid : availableGrid.Grid;

            RootListDAO<TField, TDAO> selectedList = sourceGrid.GetSelectedItems();
            destGrid.ItemsList.Modified = false;
            destGrid.BeginUpdate();

            try
            {
                if (force)
                {
                    destGrid.ItemsList.AddRange(selectedList);

                    foreach (DataGridViewRow row in sourceGrid.GridView.SelectedRows)
                        sourceGrid.GridView.Rows.RemoveAt(row.Index);

                    sourceGrid.ItemsList.RemoveAll((i) => selectedList.Contains(i), false);
                }
                else
                {
                    CanSelectResult canSelect = CanSelectResult.Available;

                    foreach (TDAO item in selectedList)
                    {
                        canSelect = select
                            ? ChooserParams.CanSelectItem is null
                                ? CanSelectResult.Available
                                : ChooserParams.CanSelectItem.Invoke(item, selectedList, this)
                            : ChooserParams.CanUnselectItem is null
                                ? CanSelectResult.Available
                                : ChooserParams.CanUnselectItem.Invoke(item, selectedList, this);

                        switch (canSelect)
                        {
                            case CanSelectResult.Return:
                                return;
                            case CanSelectResult.Continue:
                                continue;
                        }

                        destGrid.ItemsList.Add(item);
                        sourceGrid.GridView.Rows.RemoveAt(sourceGrid.GetRowIndex(item));
                        sourceGrid.ItemsList.Remove(item, false);
                    }
                }

                if (force || destGrid.ItemsList.Modified)
                {
                    destGrid.ItemsList.Sort(DataManager.DefaultSorting<TField, TDAO>()?.SortingsList);
                    destGrid.Fill();
                }
            }
            finally
            {
                destGrid.EndUpdate();
            }

            if (!force)
            {
                if (select)
                    ChooserParams.CompleteSelect?.Invoke(this, EventArgs.Empty);
                else ChooserParams.CompleteUnselect?.Invoke(this, EventArgs.Empty);

                Modified = Modified || !force;
            }
        }

        protected override void PrepareDialog(OxPanelViewer dialog)
        {
            base.PrepareDialog(dialog);
            dialog.Sizable = true;
            dialog.CanMaximize = true;
            dialog.Shown += (s, e) => RecalcGridsSizes();
            dialog.SizeChanged += (s, e) => RecalcGridsSizes();
            availablePlace.SizeChanged += (s, e) => RecalcGridsSizes();

            if (DataManager.ListController<TField, TDAO>().AvailableQuickFilter)
                availableGrid.QuickFilter.Width = availablePlace.Width;

            Modified = false;
        }

        private void RecalcGridsSizes()
        {
            selectedPlace.Width =
                OxSH.CenterOffset(
                    PanelViewer is not null
                        ? PanelViewer.Width
                        : Width,
                    buttonsPanel.Width
                );

            selectButton.Top = 
                OxSH.Sub(
                    OxSH.Half(buttonsPanel.Height), 
                    selectButton.Height,
                    50
                );
            unSelectButton.Top = OxSH.Add(selectButton.Bottom, 8);

            if (DataManager.ListController<TField, TDAO>().AvailableQuickFilter)
                availableGrid.QuickFilter.Width = availablePlace.Width;
        }

        public bool Modified { get; private set; }

        public static bool ChooseItems(Control owner, ItemsChooserParams<TField, TDAO> chooserParams, out RootListDAO<TField, TDAO> selection)
        {
            ItemsChooser<TField, TDAO> chooser = new(chooserParams);

            selection = new();

            bool result = chooser.ShowAsDialog(
                    owner, 
                    OxDialogButton.OK | OxDialogButton.Cancel
                ) is DialogResult.OK 
                && chooser.Modified;

            if (result)
                selection.LinkedCopyFrom(chooser.selectedGrid.ItemsList);

            return result;
        }
    }
}