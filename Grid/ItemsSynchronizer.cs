using OxDAOEngine.Data;
using OxLibrary.Controls;
using OxLibrary;
using OxLibrary.Panels;
using OxDAOEngine.Data.Types;
using OxLibrary.Forms;

namespace OxDAOEngine.Grid
{
    public class ItemsSynchronizer<TField, TDAO> : OxPanel
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        private readonly TDAO LeftItem;
        private readonly TDAO RightItem;

        private readonly OneItemGrid<TField, TDAO> LeftItemGrid = new(true);
        private readonly OneItemGrid<TField, TDAO> RightItemGrid = new(true);

        private readonly OxFrameWithHeader LeftPlace = new();
        private readonly OxFrameWithHeader RightPlace = new();

        private readonly OxIconButton NotEqualsButton = new(OxIcons.NotEqual, 54);
        private readonly OxIconButton LeftToRightButton = new(OxIcons.Right, 54);
        private readonly OxIconButton RightToLeftButton = new(OxIcons.Left, 54);

        private readonly OxPanel ButtonsPanel = new(new(64, 1));
        private readonly List<TField> EqualFields = new();

        protected override Bitmap? GetIcon() => OxIcons.Replace;
        public ItemsSynchronizer(TDAO leftObject, TDAO rightObject) : base(new(1280, 800))
        {
            LeftItem = leftObject;
            RightItem = rightObject;

            Text = $"{DataManager.ListController<TField, TDAO>().ListName} synchronizer";
            LeftPlace.Text = LeftItem.FullTitle();
            RightPlace.Text = RightItem.FullTitle();

            NotEqualsButton.ToolTipText = "Show only difference";
            NotEqualsButton.FreezeHovered = false;
            LeftToRightButton.ToolTipText = $"Copy value\n\tfrom {LeftItem.FullTitle()}\n\tto {RightItem.FullTitle()}";
            RightToLeftButton.ToolTipText = $"Copy value\n\tfrom {RightItem.FullTitle()}\n\tto {LeftItem.FullTitle()}";

            LeftItemGrid.Scroll += LeftScrollHandler;
            LeftItemGrid.GetFlagValue += GetFlagValueHandler;
            LeftItemGrid.SelectionChanged += SelectionChangedHandler;

            RightItemGrid.Scroll += RightScrollHandler;
            RightItemGrid.GetFlagValue += GetFlagValueHandler;
            RightItemGrid.SelectionChanged += SelectionChangedHandler;

            FillData();
            FillEqualsFields();
        }

        private void FillEqualsFields()
        {
            foreach (TField field in DataManager.FieldHelper<TField>().SynchronizedFields)
            {
                object? leftValue = LeftItem[field];
                object? rightValue = RightItem[field];

                if ((leftValue is null 
                        && rightValue is null) ||
                    (leftValue is not null 
                        && leftValue.Equals(rightValue)))
                    EqualFields.Add(field);
            }
        }

        private void FillData()
        {
            LeftItemGrid.Fill(LeftItem);
            RightItemGrid.Fill(RightItem);
            LeftItemGrid.RecalcFlags();
        }

        private bool SelectionChangedProcess = false;

        private void SelectionChangedHandler(object? sender, EventArgs e)
        {
            LeftToRightButton.Enabled = LeftItemGrid.SelectionExists;
            RightToLeftButton.Enabled = RightItemGrid.SelectionExists;

            if (SelectionChangedProcess)
                return;

            SelectionChangedProcess = true;

            try
            {
                if (LeftItemGrid.GridView.Equals(sender))
                {
                    if (LeftItemGrid.SelectionExists)
                        RightItemGrid.SelectedField = LeftItemGrid.SelectedField;
                }
                else
                    if (RightItemGrid.SelectionExists)
                        LeftItemGrid.SelectedField = RightItemGrid.SelectedField;
            }
            finally
            {
                SelectionChangedProcess = false;
            }

            string leftToRightField = $"{(LeftItemGrid.SelectionExists ? TypeHelper.Name(LeftItemGrid.SelectedField) : string.Empty)} value";
            LeftToRightButton.ToolTipText = $@"Copy {leftToRightField} 
	            from {LeftItem.FullTitle()}
	            to {RightItem.FullTitle()}";
            string rightToRightField = $"{(RightItemGrid.SelectionExists ? TypeHelper.Name(RightItemGrid.SelectedField) : string.Empty)} value";
            RightToLeftButton.ToolTipText = $@"Copy {rightToRightField}
	            from {RightItem.FullTitle()}
	            to {LeftItem.FullTitle()}";
        }

        private void LeftScrollHandler(object? sender, EventArgs e) =>
            SynchronizeScroll(LeftItemGrid, RightItemGrid);

        private void RightScrollHandler(object? sender, EventArgs e) => 
            SynchronizeScroll(RightItemGrid, LeftItemGrid);

#pragma warning disable CA1822 // Mark members as static
        private void SynchronizeScroll(OneItemGrid<TField, TDAO> source, OneItemGrid<TField, TDAO> dest) =>
            dest.FirstDisplayedScrollingRowIndex = source.FirstDisplayedScrollingRowIndex;
#pragma warning restore CA1822 // Mark members as static

        protected override void PrepareInnerComponents()
        {
            base.PrepareInnerComponents();

            LeftPlace.Parent = this;
            LeftPlace.Dock = OxDock.Fill;
            LeftPlace.Header.Underline.Visible = false;
            LeftItemGrid.Parent = LeftPlace;
            LeftItemGrid.Dock = OxDock.Fill;

            ButtonsPanel.Parent = this;
            ButtonsPanel.Dock = OxDock.Right;

            NotEqualsButton.Parent = ButtonsPanel;
            NotEqualsButton.Click += NotEqualsButtonHandler;
            NotEqualsButton.Left = 4;
            NotEqualsButton.Size = new(54, 38);
            NotEqualsButton.HiddenBorder = false;

            LeftToRightButton.Parent = ButtonsPanel;
            LeftToRightButton.Click += LeftToRightHandler;
            LeftToRightButton.Left = 4;
            LeftToRightButton.Size = new(54, 38);
            LeftToRightButton.HiddenBorder = false;

            RightToLeftButton.Parent = ButtonsPanel;
            RightToLeftButton.Click += RightToLeftHandler;
            RightToLeftButton.Left = 4;
            RightToLeftButton.Size = new(54, 38);
            RightToLeftButton.HiddenBorder = false;

            NotEqualsButton.Top =
                (short)(ButtonsPanel.Height / 2
                - NotEqualsButton.Height
                - NotEqualsButton.Height / 2
                - 4);
            LeftToRightButton.Top = (short)(NotEqualsButton.Bottom + 8);
            RightToLeftButton.Top = (short)(LeftToRightButton.Bottom + 8);

            RightPlace.Parent = this;
            RightPlace.Dock = OxDock.Right;
            RightPlace.Header.Underline.Visible = false;
            RightItemGrid.Parent = RightPlace;
            RightItemGrid.Dock = OxDock.Fill;
        }

        private void NotEqualsButtonHandler(object? sender, EventArgs e)
        {
            NotEqualsButton.FreezeHovered = !NotEqualsButton.FreezeHovered;

            if (NotEqualsButton.FreezeHovered)
            {
                NotEqualsButton.ToolTipText = "Show all";
                LeftItemGrid.HideFields(EqualFields);
                RightItemGrid.HideFields(EqualFields);
            }
            else
            {
                NotEqualsButton.ToolTipText = "Show only difference";
                LeftItemGrid.ShowAllFields();
                RightItemGrid.ShowAllFields();
            }
        }

        private bool GetFlagValueHandler(TField field, object? value)
        {
            object? leftValue = LeftItemGrid.GetValue(field);
            object? rightValue = RightItemGrid.GetValue(field);

            return 
                (leftValue is null 
                    && rightValue is null) 
                || (leftValue is not null 
                    && leftValue.Equals(rightValue));
        }

        public override void PrepareColors()
        {
            base.PrepareColors();

            if (LeftItem is null)
                return;

            LeftPlace.BaseColor = LeftItem.BackColor;
            LeftItemGrid.BaseColor = LeftItem.BackColor;
            RightPlace.BaseColor = RightItem.BackColor;
            RightItemGrid.BaseColor = RightItem.BackColor;

            ButtonsPanel.BaseColor = BaseColor;
            NotEqualsButton.BaseColor = BaseColor;
            LeftToRightButton.BaseColor = BaseColor;
            RightToLeftButton.BaseColor = BaseColor;
        }

        private void RightToLeftHandler(object? sender, EventArgs e) =>
            Synchronize(RightItemGrid, LeftItemGrid);

        private void LeftToRightHandler(object? sender, EventArgs e) => 
            Synchronize(LeftItemGrid, RightItemGrid);

#pragma warning disable CA1822 // Mark members as static
        private void Synchronize(OneItemGrid<TField, TDAO> source, OneItemGrid<TField, TDAO> dest)
#pragma warning restore CA1822 // Mark members as static
        {
            dest.SetValue(
                source.SelectedField,
                source.GetValue(source.SelectedField)
            );
            source.RecalcFlags();
        }

        protected override void PrepareDialog(OxPanelViewer dialog)
        {
            base.PrepareDialog(dialog);
            dialog.Sizable = true;
            dialog.CanMaximize = true;
            dialog.Shown += (s, e) => RecalcGridsSizes();
            dialog.SizeChanged += (s, e) => RecalcGridsSizes();
            dialog.DialogButtons = OxDialogButton.Apply | OxDialogButton.Cancel;
            LeftPlace.SizeChanged += (s, e) => RecalcGridsSizes();
        }

        private void RecalcGridsSizes()
        {
            RightPlace.Width =
                (short)(((PanelViewer is not null
                        ? PanelViewer.Width
                        : Width) 
                        - ButtonsPanel.Width) 
                    / 2);
            NotEqualsButton.Top =
                (short)(ButtonsPanel.Height / 2
                - NotEqualsButton.Height
                - NotEqualsButton.Height / 2
                - 4);
            LeftToRightButton.Top = (short)(NotEqualsButton.Bottom + 8);
            RightToLeftButton.Top = (short)(LeftToRightButton.Bottom + 8);
        }

        private void ApplySynchronize()
        {
            foreach (TField field in DataManager.FieldHelper<TField>().SynchronizedFields)
            {
                if (EqualFields.Contains(field))
                    continue;

                object? leftValue = LeftItemGrid.GetValue(field);

                if ((LeftItem[field] is null 
                        && leftValue is not null)
                    || (LeftItem[field] is not null 
                        && !LeftItem[field]!.Equals(leftValue)))
                    LeftItem[field] = LeftItemGrid.GetValue(field);

                object? rightValue = RightItemGrid.GetValue(field);

                if ((RightItem[field] is null 
                        && rightValue is not null)
                    || (RightItem[field] is not null 
                        && !RightItem[field]!.Equals(rightValue)))
                    RightItem[field] = RightItemGrid.GetValue(field);
            }
        }

        public static bool SynchronizeItems(TDAO leftItem, TDAO rightItem, OxPanel owner)
        {
            ItemsSynchronizer<TField, TDAO> synchronizer = new(leftItem, rightItem)
            {
                BaseColor = owner.BaseColor
            };
            bool result = synchronizer.ShowAsDialog(owner) is DialogResult.OK;

            if (result)
                synchronizer.ApplySynchronize();

            return result;
        }
    }
}