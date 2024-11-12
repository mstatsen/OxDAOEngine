using OxLibrary;
using OxLibrary.Controls;
using OxLibrary.Panels;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Filter;

namespace OxDAOEngine.ControlFactory.Controls
{
    public delegate int GetMaximumCount();

    public abstract class ListItemsControl<TList, TItem, TEditor, TField, TDAO> : 
        CustomListControl<TField, TDAO, TList, TItem>, 
        IListItemsControl<TField, TDAO>
        where TList : ListDAO<TItem>, new()
        where TItem : DAO, new()
        where TEditor : ListItemEditor<TItem, TField, TDAO>, new()
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        private readonly OxClickFrameList Buttons = new();
        private readonly OxClickFrameList EnabledWhenItemSelected = new();
        private readonly Dictionary<OxClickFrame, ListControlButtonEffect> ButtonEffects = new();

        public OxPane ButtonsPanel { get; private set; } = new();
        public OxPane ControlPanel { get; private set; } = new();
        public IItemListControl ListBox { get; internal set; } = new OxListBox()
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.None
        };

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
            editor.ParentItem = ParentItem;
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
                if (editingType == TypeOfEditorShow.Add ||
                    !EqualsItems(item, SelectedItem))
                    existingItems.Add(item);

            return existingItems;
        }

        private void AddItem()
        {
            if (!AddButton.Enabled 
                || readOnly 
                || AllItemsAdded)
                return;

            TItem? item = Editor(TypeOfEditorShow.Add).Add();

            if (item == null)
                return;

            SetValuePart(item);
            ResortValue();
            ListBox.SelectedItem = item;
            InvokeValueChangeHandler();
            ItemAdded?.Invoke(item, EventArgs.Empty);
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
            (TItem?)ListBox.SelectedItem;

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
            
            int selectedIndex = ListBox.SelectedIndex;
            ListBox.BeginUpdate();
            try
            {
                ListBox.ClearSelected();
                ListBox.UpdateItem(selectedIndex, item);
                ResortValue();
                ListBox.SelectedItem = item;
                ItemEdited?.Invoke(item, EventArgs.Empty);
            }
            finally
            {
                ListBox.EndUpdate();
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

            ListBox.BeginUpdate();

            try
            {
                int removeIndex = ListBox.SelectedIndex;

                if (removeIndex < 0)
                    return;

                ListBox.RemoveAt(removeIndex);

                if (removeIndex == ListBox.Count)
                    removeIndex--;

                if (removeIndex > -1)
                    ListBox.SelectedIndex = removeIndex;
            }
            finally
            {
                ListBox.EndUpdate();
            }

            InvokeValueChangeHandler();
            ItemRemoved?.Invoke(this, EventArgs.Empty);
        }

        private bool AllItemsAdded => 
            ListBox.Count == (
                GetMaximumCount != null
                    ? GetMaximumCount()
                    : MaximumItemsCount
            );

        protected virtual void EnableControls()
        {
            ButtonsPanel.Visible = 
                !(readOnly && ReadonlyMode == ReadonlyMode.ViewAsReadonly) 
                || ButtonEffects.ContainsValue(ListControlButtonEffect.View);

            AddButton.Visible = !readOnly;
            DeleteButton.Visible = !readOnly;
            RecalcEditButtonVisible();
            AddButton.Enabled = !AllItemsAdded;

            foreach (OxClickFrame eControl in EnabledWhenItemSelected)
                eControl.Enabled = ListBox.SelectedIndex > -1 
                    && (FixedItems == null 
                        || !FixedItems.Contains(ListBox.SelectedItem));
        }

        protected void PrepareEditButton(OxIconButton button, EventHandler handler,
            bool onlyForSelectedItem = false, int index = -1) =>
            PrepareButton(button, handler, ListControlButtonEffect.Edit, onlyForSelectedItem, index);

        protected void PrepareViewButton(OxIconButton button, EventHandler handler,
            bool onlyForSelectedItem = false, int index = -1) =>
            PrepareButton(button, handler, ListControlButtonEffect.View, onlyForSelectedItem, index);

        protected void PrepareButton(
            OxIconButton button,
            EventHandler handler, 
            ListControlButtonEffect effect = ListControlButtonEffect.Edit, 
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
            DeleteButton.ToolTipText = $"Delete {ItemName().ToLower()}";
            EditButton.ToolTipText = $"Edit {ItemName().ToLower()}";
            PrepareEditButton(AddButton, (s, e) => AddItem());
            PrepareEditButton(DeleteButton, (s, e) => RemoveItem(), true);
            PrepareEditButton(EditButton, (s, e) => EditItem(), true);
        }

        private readonly OxIconButton AddButton = CreateButton(OxIcons.Plus);
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
            PrepareButtonsPanel();
            InitControl();
            EnableControls();
        }

        protected void InitControl()
        {
            ListBox.Parent = ControlPanel;
            ListBox.SelectedIndexChanged += (s, e) => EnableControls();
            ListBox.DoubleClick += (s, e) => EditItem();
            ListBox.Click += ListClickHandler;
            ListBox.KeyUp += ListBoxKeyUpHandler;
            ListBox.CheckIsHighPriorityItem += ListBoxCheckIsHighPriorityItemHandler;
            ListBox.CheckIsMandatoryItem += ListBoxCheckIsMandatoryItemHandler;
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
            if (ListBox.SelectedItem == null 
                && ListBox.Count > 0)
                ListBox.SelectedIndex = 0;
        }

        protected override void SetValuePart(TItem valuePart)
        {
            ListBox.Add(valuePart);
            EnableControls();
        }

        protected override void GrabList(TList list)
        {
            list.Clear();

            foreach (object item in ListBox.ObjectList)
                list.Add(((TItem)item).GetCopy<TItem>());
        }

        protected override Control GetControl() => (Control)ListBox;

        protected override void ClearValue()
        {
            ListBox.ClearSelected();
            ListBox.Clear();
            InvokeValueChangeHandler();
        }

        protected override void RecalcControls()
        {
            base.RecalcControls();
            ListBox.Height = ControlPanel.Height;
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