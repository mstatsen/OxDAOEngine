using OxLibrary;
using OxLibrary.Controls;
using OxLibrary.Panels;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Filter;

namespace OxDAOEngine.ControlFactory.Controls
{
    public delegate int GetMaximumCount();

    public abstract class CustomItemsContainerControl<TList, TItem, TItemsContainer, TEditor, TField, TDAO> : 
        CustomItemsControl<TField, TDAO, TList, TItem>, 
        IItemsContainerControl<TField, TDAO>
        where TList : ListDAO<TItem>, new()
        where TItem : DAO, new()
        where TEditor : CustomItemEditor<TItem, TField, TDAO>, new()
        where TItemsContainer : IItemsContainer, new()
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        private readonly OxClickFrameList Buttons = new();
        private readonly OxClickFrameList EnabledWhenItemSelected = new();
        private readonly Dictionary<OxClickFrame, ItemsContainerButtonEffect> ButtonEffects = new();

        public OxPane ButtonsPanel { get; private set; } = new();
        public OxPane ControlPanel { get; private set; } = new();
        public IItemsContainer ItemsContainer { get; internal set; } = new TItemsContainer()
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.None
        };

        protected virtual void PrepareItemsContainer() { }

        protected abstract string ItemName();

        private const int ButtonSpace = (int)OxSize.Medium;
        private const int ButtonWidth = 28;
        private const int ButtonHeight = 20;

        private TEditor? editor;

        private TEditor Editor(TypeOfEditorShow type)
        {
            editor ??= new TEditor().Init<TEditor>(Context);
            editor.OwnerControl = this;
            editor.BaseColor = Colors.Lighter(Context.Scope == ControlScope.Editor ? 0 : 1);
            PrepareEditor(editor);
            editor.OwnerDAO = OwnerDAO;
            editor.ParentItem = SelectedItem;
            editor.ExistingItems = GetExistingItems(type).ObjectList;
            editor.Filter = Filter;
            editor.Text = ItemName();
            editor.RenewData();
            return editor;
        }

        protected virtual void PrepareEditor(TEditor editor) { }

        public IMatcher<TField>? Filter { get; set; }

        protected virtual bool EqualsItems(TItem? leftItem, TItem? rightItem) => 
            (leftItem is null && rightItem is null)
            || (leftItem is not null && leftItem.Equals(rightItem));

        protected TList GetExistingItems(TypeOfEditorShow editingType)
        {
            TList existingItems = new();

            foreach (TItem item in Value)
                if (editingType != TypeOfEditorShow.Edit 
                    || !EqualsItems(item, SelectedItem))
                    existingItems.Add(item);

            return existingItems;
        }

        protected virtual void AddChildItem() =>
            AddItem(TypeOfEditorShow.AddChild);

        private bool AddItem(TypeOfEditorShow addType = TypeOfEditorShow.Add)
        {
            if (!AddButton.Enabled 
                || readOnly 
                || AllItemsAdded)
                return false;

            TItem? item = Editor(addType).Add();

            if (item == null)
                return false;

            if (addType == TypeOfEditorShow.AddChild
                && item is ITreeItemDAO<TItem> treeItem)
                treeItem.Parent = SelectedItem;

            SetValuePart(item);

            if (addType == TypeOfEditorShow.Add)
                ResortValue();

            ItemsContainer.SelectedItem = item;
            InvokeValueChangeHandler();
            ItemAdded?.Invoke(item, EventArgs.Empty);
            return true;
        }

        private bool valueChangeHandlerEnabled = true;

        public void DisableValueChangeHandler() => 
            valueChangeHandlerEnabled = false;
        
        public void EnableValueChangeHandler() => 
            valueChangeHandlerEnabled = true;

        private void InvokeValueChangeHandler()
        {
            if (valueChangeHandlerEnabled)
                ValueChangeHandler?.Invoke(this, EventArgs.Empty);

            EnableControls();
        }

        protected TItem? SelectedItem => 
            (TItem?)ItemsContainer.SelectedItem;

        private void EditItem()
        {
            if (!EditButton.Enabled || 
                (readOnly && ReadonlyMode == ReadonlyMode.ViewAsReadonly))
                return;

            TItem? item = SelectedItem;

            if (item == null)
                return;

            if (Editor(TypeOfEditorShow.Edit).Edit(item, readOnly) != DialogResult.OK)
                return;
            
            ItemsContainer.BeginUpdate();
            try
            {
                ItemsContainer.UpdateSelectedItem(item);
                ResortValue();
                ItemsContainer.SelectedItem = item;
                ItemEdited?.Invoke(item, EventArgs.Empty);
            }
            finally
            {
                ItemsContainer.EndUpdate();
            }

            InvokeValueChangeHandler();
        }

        protected new void ResortValue()
        {
            DisableValueChangeHandler();
            try
            {
                base.ResortValue();
            }
            finally
            {
                EnableValueChangeHandler();
            }
        }

        private void RemoveItem()
        {
            if (!DeleteButton.Enabled || 
                readOnly)
                return;

            ItemsContainer.BeginUpdate();

            try
            {
                RemoveCurrentItemFromContainer();
            }
            finally
            {
                ItemsContainer.EndUpdate();
            }

            InvokeValueChangeHandler();
            ItemRemoved?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void RemoveCurrentItemFromContainer() =>
            ItemsContainer.RemoveCurrent();

        private bool AllItemsAdded => 
            ItemsContainer.Count == (
                GetMaximumCount != null
                    ? GetMaximumCount()
                    : MaximumItemsCount
            );

        protected virtual void EnableControls()
        {
            ButtonsPanel.Visible = 
                !(readOnly && ReadonlyMode == ReadonlyMode.ViewAsReadonly) 
                || ButtonEffects.ContainsValue(ItemsContainerButtonEffect.View);

            AddButton.Visible = !readOnly;
            AddChildButton.Visible = !readOnly && ItemsContainer.AvailableChilds;
            DeleteButton.Visible = !readOnly;
            RecalcEditButtonVisible();
            AddButton.Enabled = !AllItemsAdded;
            AddChildButton.Enabled = !AllItemsAdded && SelectedItem != null;

            foreach (OxClickFrame eControl in EnabledWhenItemSelected)
                eControl.Enabled = ItemsContainer.SelectedIndex > -1 
                    && (FixedItems == null 
                        || !FixedItems.Contains(ItemsContainer.SelectedItem));
        }

        protected void PrepareEditButton(OxIconButton button, EventHandler handler,
            bool onlyForSelectedItem = false, int index = -1) =>
            PrepareButton(button, handler, ItemsContainerButtonEffect.Edit, onlyForSelectedItem, index);

        protected void PrepareViewButton(OxIconButton button, EventHandler handler,
            bool onlyForSelectedItem = false, int index = -1) =>
            PrepareButton(button, handler, ItemsContainerButtonEffect.View, onlyForSelectedItem, index);

        protected void PrepareButton(
            OxIconButton button,
            EventHandler handler, 
            ItemsContainerButtonEffect effect = ItemsContainerButtonEffect.Edit, 
            bool onlyForSelectedItem = false, 
            int index = -1)
        {
            button.Parent = ButtonsPanel;
            button.Height = ButtonHeight;
            button.BaseColor = BaseColor;
            button.HiddenBorder = false;

            if (handler != null)
                button.Click += handler;

            if (index == -1)
                index = Buttons.Count;

            Buttons.Insert(index, button);

            if (onlyForSelectedItem)
                EnabledWhenItemSelected.Add(button);

            ButtonEffects.Add(button, effect);
        }

        protected static OxIconButton CreateButton(Bitmap? icon) =>
            new(icon, ButtonWidth);

        protected virtual void InitButtons()
        {
            AddButton.ToolTipText = $"Add {ItemName().ToLower()}";
            AddChildButton.ToolTipText = $"Add child {ItemName().ToLower()}";
            DeleteButton.ToolTipText = $"Delete {ItemName().ToLower()}";
            EditButton.ToolTipText = $"Edit {ItemName().ToLower()}";
            PrepareEditButton(AddButton, (s, e) => AddItem());
            PrepareEditButton(AddChildButton, (s, e) => AddChildItem());
            PrepareEditButton(DeleteButton, (s, e) => RemoveItem(), true);
            PrepareEditButton(EditButton, (s, e) => EditItem(), true);
        }

        private readonly OxIconButton AddButton = CreateButton(OxIcons.Plus);
        private readonly OxIconButton AddChildButton = CreateButton(OxIcons.AddChild);
        private readonly OxIconButton DeleteButton = CreateButton(OxIcons.Minus);
        private readonly OxIconButton EditButton = CreateButton(OxIcons.Pencil);

        private void SetEditButtonVisible(bool value)
        {
            if (EditButton == null 
                || EditButton.Visible == value)
                return;

            if (!readOnly 
                || !value)
                EditButton.Visible = value;
        }

        private void PrepareButtonsPanel()
        {
            ButtonsPanel.Parent = this;
            ButtonsPanel.Dock = DockStyle.Right;
            ButtonsPanel.Width = ButtonWidth + ButtonSpace * 2;
            OxBorder.NewTop(ButtonsPanel, Color.Transparent, ButtonSpace);
            InitButtons();
            LayoutButtons();
        }

        private void LayoutButtons()
        {
            int calcedTop = ButtonSpace;

            foreach (OxClickFrame button in Buttons)
            {
                if (!button.Visible)
                    continue;

                button.Left = ButtonSpace;
                button.Top = calcedTop;
                calcedTop = button.Bottom + ButtonSpace;
            }
        }

        private void PrepareControlPanel()
        {
            ControlPanel.Parent = this;
            ControlPanel.Dock = DockStyle.Fill;
            ControlPanel.AutoScroll = true;
        }

        protected sealed override void InitComponents()
        {
            PrepareControlPanel();
            PrepareItemsContainer();
            PrepareButtonsPanel();
            InitControl();
            EnableControls();
        }

        protected void InitControl()
        {
            ItemsContainer.Parent = ControlPanel;
            ItemsContainer.SelectedIndexChanged += (s, e) => EnableControls();
            ItemsContainer.DoubleClick += (s, e) => EditItem();
            ItemsContainer.Click += ListClickHandler;
            ItemsContainer.KeyUp += ListBoxKeyUpHandler;
            ItemsContainer.CheckIsHighPriorityItem += ListBoxCheckIsHighPriorityItemHandler;
            ItemsContainer.CheckIsMandatoryItem += ListBoxCheckIsMandatoryItemHandler;
        }

        private bool ListBoxCheckIsMandatoryItemHandler(object item) =>
            IsMandatoryItem((TItem)item);

        private bool ListBoxCheckIsHighPriorityItemHandler(object item) => 
            IsHighPriorityItem((TItem)item);

        protected virtual bool IsMandatoryItem(TItem item) =>
            FixedItems != null && FixedItems.Contains(item);

        protected virtual bool IsHighPriorityItem(TItem item) => false;

        private void ListBoxKeyUpHandler(object? sender, KeyEventArgs e)
        {
            if (e.Handled)
                return;

            switch (e.KeyCode)
            {
                case Keys.Delete:
                    RemoveItem();
                    break;
                case Keys.Insert:
                    AddItem();
                    break;
                case Keys.Enter:
                    EditItem();
                    break;
            }
        }

        private void ListClickHandler(object? sender, EventArgs e)
        {
            if (ItemsContainer.SelectedItem == null 
                && ItemsContainer.Count > 0)
                ItemsContainer.SelectedIndex = 0;
        }

        protected virtual void AddValueToItemsContainer(TItem valuePart) =>
            ItemsContainer.Add(valuePart);
        protected sealed override void SetValuePart(TItem valuePart)
        {
            AddValueToItemsContainer(valuePart);
            EnableControls();
        }

        protected override void GrabList(TList list)
        {
            list.Clear();

            foreach (object item in ItemsContainer.ObjectList)
                list.Add(((TItem)item).GetCopy<TItem>());
        }

        protected override Control GetControl() => ItemsContainer.AsControl();

        protected override void ClearValue()
        {
            ItemsContainer.ClearSelected();
            ItemsContainer.Clear();
            InvokeValueChangeHandler();
        }

        protected override void RecalcControls()
        {
            base.RecalcControls();
            ItemsContainer.Height = ControlPanel.Height;
        }

        protected override void SetControlColor(Color value)
        {
            base.SetControlColor(value);
            BaseColor = value;
        }

        protected override void PrepareColors()
        {
            base.PrepareColors();
            SetPaneBaseColor(ControlPanel, BaseColor);
            SetPaneBaseColor(ButtonsPanel, BaseColor);
            SetControlBackColor(Control, Colors.Lighter(7));

            if (Buttons == null)
                return;

            foreach (OxIconButton button in Buttons.Cast<OxIconButton>())
                button.BaseColor = BaseColor;
        }

        public GetMaximumCount? GetMaximumCount;

        protected virtual int MaximumItemsCount => -1;

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            RecalcEditButtonVisible();
        }

        private void RecalcEditButtonVisible()
        {
            if (ButtonsPanel == null)
                return;

            SetEditButtonVisible(
                !(readOnly && ReadonlyMode == ReadonlyMode.ViewAsReadonly) 
                && (CalcedButtonsHeight <= ButtonsPanel.Height)
            );
            LayoutButtons();
        }

        private int CalcedButtonsHeight
        {
            get
            {
                int calcedHeight = 0;

                foreach (OxClickFrame button in Buttons)
                    if (button.Visible)
                        calcedHeight += button.Height + ButtonSpace;

                if (EditButton != null && !EditButton.Visible)
                    calcedHeight += EditButton.Height + ButtonSpace;

                return calcedHeight;
            }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            RecalcEditButtonVisible();
        }

        protected override bool GetReadOnly() => 
            readOnly;

        protected override void SetReadOnly(bool value)
        {
            readOnly = value;
            EnableControls();
            LayoutButtons();
        }

        private bool readOnly = false;

        public EventHandler? itemAdded;
        public EventHandler? itemRemoved;

        public EventHandler? ItemAdded 
        { 
            get => itemAdded; 
            set => itemAdded = value; 
        }

        public EventHandler? ItemRemoved
        {
            get => itemRemoved;
            set => itemRemoved = value;
        }
    }
}