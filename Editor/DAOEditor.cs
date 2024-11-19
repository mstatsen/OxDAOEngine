using OxLibrary;
using OxLibrary.Dialogs;
using OxLibrary.Panels;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Fields;
using OxLibrary.Controls;
using OxDAOEngine.Grid;

namespace OxDAOEngine.Editor
{
    public abstract partial class DAOEditor<TField, TDAO, TFieldGroup> : OxDialog
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
        where TFieldGroup : notnull, Enum
    {
        private bool readOnly = false;
        public bool ReadOnly
        {
            get => readOnly;
            set => readOnly = value;
        }

        protected bool CreatingProcess = false;

        public DAOEditor() : base()
        {
            CreatingProcess = true;

            try
            {
                StartPosition = FormStartPosition.CenterScreen;
                InitializeComponent();
                CreatePanels();
                MainPanel.Colors.BaseColorChanged += FormColorChanged;
                FormClosed += FormClosedHandler;
                MainPanel.Header.AddToolButton(prevButton);
                MainPanel.Header.AddToolButton(nextButton);
                FieldHelper<TField> fieldHelper = DataManager.FieldHelper<TField>();
                OxIconButton idButton = new(OxIcons.Key, 28)
                {
                    ToolTipText = $"View {fieldHelper.Name(fieldHelper.UniqueField)}"
                };
                idButton.Click += (s, e) => uniqueKeyViewer.View(Item, this);
                MainPanel.Header.AddToolButton(idButton);
                MainPanel.SetHeaderContentSize(35);
            }
            finally
            {
                CreatingProcess = false;
            }
        }

        private readonly UniqueKeyViewer<TField, TDAO> uniqueKeyViewer = new();

        protected virtual void CreatePanels()
        {
            PreparePanels();
            GroupParents.Add(MainPanel);
            SetParentsColor();
            CreateGroups();
            PlaceGroups();
            SetGroupsColor();
            SetGroupCaptions();
            SetMargins();
            SetPaddings();
            SetHandlers();
        }

        protected virtual void SetGroupCaptions() { }

        private void SetGroupsColor()
        {
            foreach (OxFrame frame in Groups.Values)
                frame.BaseColor = MainPanel.Colors.Lighter();
        }

        private void PlaceGroups()
        {
            foreach (var group in Groups)
                group.Value.Parent = GroupParent(group.Key);
        }

        private void CreateGroups()
        {
            foreach (TFieldGroup group in EditedGroups)
                CreateGroup(group);
        }

        private readonly FieldGroupFrames<TField, TFieldGroup> groups = new();

        protected virtual void PreparePanels() { }

        private void SetHandlers()
        {
            foreach (OxPane pane in Groups.Values)
                pane.Resize += (s, e) => InvalidateSize();
        }

        protected virtual void SetPaddings() { }

        private void SetMargins()
        {
            foreach (var item in Groups)
                SetFrameMargin(item.Key, item.Value);
        }

        protected virtual void SetFrameMargin(TFieldGroup group, OxFrame frame)
        {
            frame.Margins.LeftOx = OxSize.M;
            frame.Margins.TopOx = OxSize.M;
            frame.Margins.RightOx = OxSize.None;
            frame.Margins.BottomOx = OxSize.None;
        }

        private static readonly IListController<TField, TDAO> listController 
            = DataManager.ListController<TField, TDAO>();

        protected OxIconButton prevButton = new(OxIcons.Up, 28)
        {
            ToolTipText = $"Prevous {listController.ItemName}"
        };
        protected OxIconButton nextButton = new(OxIcons.Down, 28)
        {
            ToolTipText = $"Next {listController.ItemName}"
        };

        private ItemsGrid<TField, TDAO>? parentGrid;

        public ItemsGrid<TField, TDAO>? ParentGrid 
        { 
            get => parentGrid;
            set => SetParentGrid(value);
        }

        private void SetParentGrid(ItemsGrid<TField, TDAO>? value)
        {
            parentGrid = value;
            PrepareButtons();
        }

        private void PrepareButtons()
        {
            if (parentGrid is null)
            {
                prevButton.Visible = false;
                nextButton.Visible = false;
                return;
            }

            prevButton.Click -= PrevClickHandler;
            prevButton.Click += PrevClickHandler;
            nextButton.Click -= NextClickHandler;
            nextButton.Click += NextClickHandler;
            SetButtonsEnabled();
        }

        private void SetButtonsEnabled()
        {
            if (parentGrid is null)
                return;

            prevButton.Enabled = !parentGrid.IsFirstRecord;
            nextButton.Enabled = !parentGrid.IsLastRecord;
        }

        private void NextClickHandler(object? sender, EventArgs e)
        {
            if (parentGrid is null 
                || !ReadyToChangeItem())
                return;

            Item = parentGrid.GoNext();
        }

        private void PrevClickHandler(object? sender, EventArgs e)
        {
            if (parentGrid is null 
                || !ReadyToChangeItem())
                return;

            Item = parentGrid.GoPrev();
        }

        private bool ReadyToChangeItem()
        {
            if (Worker.Modified)
            {
                DialogResult userConfirm = OxMessage.ShowWarning(
                    "You have uncommitted changes. Do you want to apply it?\n\n" +
                    "[Apply] - commit changes and continue\n" +
                    "[Discard] - discard changes and continue\n" +
                    "[Cancel] - continue editing the current item",
                    this,
                    OxDialogButton.Apply | OxDialogButton.Discard | OxDialogButton.Cancel
                );

                switch (userConfirm)
                {
                    case DialogResult.Cancel:
                        return false;
                    case DialogResult.OK:
                        SaveChanges();
                        break;
                }
            }

            return true;
        }

        public TDAO? Item
        {
            get => Worker.Item;
            set
            {
                Worker.Item = value;
                PrepareButtons();
                Groups.SetGroupsSize();
                RecalcPanels();
            }
        }

        public FieldGroupFrames<TField, TFieldGroup> Groups => groups;

        public override bool CanOKClose() =>
            Worker.CheckMandatoryFields();

        public override bool CanCancelClose() => 
            !Worker.Modified ||
            OxMessage.Confirmation("All uncommited changes will be lost.\nDo you really want to leave this form?", this);

        private void FormClosedHandler(object? sender, FormClosedEventArgs e)
        {
            if (DialogResult is DialogResult.OK 
                && Item is not null)
                SaveChanges();

            Worker.UnSetHandlers();
        }

        private void SaveChanges()
        {
            if (Item is null)
                return;

            DAOEntityEventHandler? savedChangeHandler = Item.ChangeHandler;
            Item.ChangeHandler = null;
            Item.StartSilentChange();

            try
            {
                Worker.GrabControls();
            }
            finally
            {
                Item.ChangeHandler = savedChangeHandler;
                Item.FinishSilentChange();
                Item.ChangeHandler?.Invoke(Item, new DAOEntityEventArgs(DAOOperation.Modify));
            }
        }

        public DAOWorker<TField, TDAO, TFieldGroup> Worker => DataManager.Worker<TField, TDAO, TFieldGroup>();

        private bool invalidateSizeInProcess = false;

        public void InvalidateSize(bool centerForm = false)
        {
            if (invalidateSizeInProcess)
                return;

            invalidateSizeInProcess = true;
            SuspendLayout();
            MainPanel.SuspendLayout();
            try
            {
                Groups.SetGroupsSize();
                RecalcPanels();
                MainPanel.ReAlign();

                if (centerForm)
                    OxControlHelper.CenterForm(this);

                Invalidate();
            }
            finally
            {
                MainPanel.ResumeLayout();
                ResumeLayout();
                invalidateSizeInProcess = false;
            }
        }

        public bool SetParentsVisible(bool forceVisible)
        {
            bool visibleChanged = false;

            foreach (var parent in GroupParents)
            {
                if (parent.Key.Equals(MainPanel))
                    continue;

                bool calcedVisible = 
                    forceVisible 
                    || parent.Value.Find(g => g.Visible) is not null;

                if (!parent.Key.Visible.Equals(calcedVisible))
                {
                    visibleChanged = true;
                    parent.Key.Visible = calcedVisible;
                    parent.Key.Update();
                    parent.Key.Parent?.Update();
                }
            }

            return visibleChanged;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            SetParentsVisible(false);
            SetGroupCaptions();
            InvalidateSize(true);
        }

        protected abstract void RecalcPanels();

        protected readonly FieldGroupHelper<TField, TFieldGroup> FieldGroupHelper =
            DataManager.FieldGroupHelper<TField, TFieldGroup>();

        private OxFrame CreateGroup(TFieldGroup group)
        {
            OxFrameWithHeader groupFrame = new()
            {
                Text = FieldGroupHelper.Name(group),
                Dock = Groups.Dock(group),
                UseDisabledStyles = false,
                BlurredBorder = true
            };
            AddFrameToParent(group, groupFrame);
            Groups.Add(group, groupFrame);
            return groupFrame;
        }

        private void FormColorChanged(object? sender, EventArgs e)
        {
            SetParentsColor();
            SetGroupsColor();
        }

        protected virtual void AddFrameToParent(TFieldGroup group, OxFrameWithHeader groupFrame)
        {
            OxPane? groupParent = GroupParent(group);

            if (groupParent is not null)
                GroupParents[groupParent].Add(groupFrame);
        }

        protected void SetParentsColor() 
        { 
            foreach (OxPanel panel in ParentPanels.Cast<OxPanel>())
                if (!panel.Equals(MainPanel))
                    panel.BaseColor = MainPanel.Colors.Lighter();
        }

        protected virtual List<TFieldGroup> EditedGroups =>
            FieldGroupHelper.EditedList();

        public readonly OxPaneDictionary GroupParents = new();
        public readonly OxPaneList ParentPanels = new();

        protected virtual OxPane? GroupParent(TFieldGroup group) 
            => MainPanel;

        protected int CalcedWidth(OxPane parentControl)
        {
            int result = 0;

            foreach (TFieldGroup group in Groups.Keys)
                if (parentControl.Equals(GroupParent(group)))
                    result = Math.Max(result, Groups.Width(group));

            return result;
        }

        protected int CalcedHeight(OxPane parentControl)
        {
            int result = 0;

            foreach (OxFrame container in GroupParents[parentControl].Cast<OxFrame>())
                result += (container.Visible ? container.Height : 0);

            return result;
        }

        protected void PrepareParentPanel(OxPanel panel, Control parent, DockStyle dock = DockStyle.Left, bool parentForGroups = true)
        {
            panel.Parent = parent;
            panel.Dock = dock;
            ParentPanels.Add(panel);
            panel.VisibleChanged += ParentPanelVisibleChangedHandler;

            if (parentForGroups)
                GroupParents.Add(panel);
        }

        private void ParentPanelVisibleChangedHandler(object? sender, EventArgs e) => InvalidateSize();
    }
}