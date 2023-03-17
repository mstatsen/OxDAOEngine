using OxLibrary;
using OxLibrary.Controls;
using OxLibrary.Panels;
using OxXMLEngine.Data;
using OxXMLEngine.Data.Filter;

namespace OxXMLEngine.ControlFactory.Controls
{
    public delegate int GetMaximumCount();
    
    public class ListItemsControl<TList, TItem, TEditor, TField, TDAO> : 
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
        public OxListBox ListBox { get; internal set; } = new()
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.None
        };

        private const int ButtonSpace = (int)OxSize.Medium;
        private const int ButtonWidth = 28;
        private const int ButtonHeight = 20;

        private TEditor? editor;

        private TEditor Editor(TypeOfEditorShow type)
        {
            if (editor == null)
                editor = new TEditor
                {
                    BaseColor = Colors.Lighter(Context.Scope == ControlScope.Editor ? 0 : 1)
                }.Init<TEditor>(Context);

            editor.ExistingItems = GetExistingItems(type).ObjectList;
            editor.Filter = Filter;
            editor.RenewData();
            return editor;
        }

        public IMatcher<TField>? Filter;

        protected virtual bool EqualsItems(TItem leftItem, TItem rightItem) =>
            leftItem.Equals(rightItem);

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
            if (readOnly)
                return;

            TItem? item = Editor(TypeOfEditorShow.Add).Add();

            if (item != null)
            {
                SetValuePart(item);
                ResortValue();
                ListBox.SelectedItem = item;
                InvokeValueChangeHandler();
            }
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

        protected TItem SelectedItem => 
            (TItem)ListBox.SelectedItem;

        private void EditItem()
        {
            if (readOnly)
                return;

            TItem item = SelectedItem;

            if (item == null)
                return;

            if (Editor(TypeOfEditorShow.Edit).Edit(item) == DialogResult.OK)
            {
                int selectedIndex = ListBox.SelectedIndex;
                ListBox.BeginUpdate();
                try
                {
                    ListBox.ClearSelected();
                    ListBox.Items[selectedIndex] = item;
                    ResortValue();
                    ListBox.SelectedItem = item;
                }
                finally
                {
                    ListBox.EndUpdate();
                }
                InvokeValueChangeHandler();
            }
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
            if (readOnly)
                return;

            ListBox.BeginUpdate();

            try
            {
                int removeIndex = ListBox.SelectedIndex;
                ListBox.Items.RemoveAt(removeIndex);

                if (removeIndex == ListBox.Items.Count)
                    removeIndex--;

                if (removeIndex > -1)
                    ListBox.SelectedIndex = removeIndex;
            }
            finally
            {
                ListBox.EndUpdate();
            }

            InvokeValueChangeHandler();
        }

        private static void SetButtonEnabled(OxClickFrame button, bool enabled)
        {
            button.Enabled = enabled;
            button.HiddenBorder = !enabled;
        }

        private bool AllItemsAdded()
        {
            int maximum = OnGetMaximumCount != null 
                ? OnGetMaximumCount() 
                : MaximumItemsCount;

            return ListBox.Items.Count < maximum || maximum == -1;
        }

        protected virtual void EnableControls()
        {
            ButtonsPanel.Visible = !readOnly ||
               ButtonEffects.ContainsValue(ListControlButtonEffect.View);

            AddButton.Visible = !readOnly;
            DeleteButton.Visible = !readOnly;
            RecalcEditButtonVisible();
            SetButtonEnabled(AddButton, AllItemsAdded());

            foreach (OxClickFrame eControl in EnabledWhenItemSelected)
                SetButtonEnabled(eControl, ListBox.SelectedIndex > -1);
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
            PrepareEditButton(AddButton, (s, e) => AddItem());
            PrepareEditButton(DeleteButton, (s, e) => RemoveItem(), true);
            PrepareEditButton(EditButton, (s, e) => EditItem(), true);
        }

        private readonly OxIconButton AddButton = CreateButton(OxIcons.plus);
        private readonly OxIconButton DeleteButton = CreateButton(OxIcons.minus);
        private readonly OxIconButton EditButton = CreateButton(OxIcons.pencil);

        private void SetEditButtonVisible(bool value)
        {
            if (EditButton == null || EditButton.Visible == value)
                return;

            if (!readOnly || !value)
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
        }

        private void ListClickHandler(object? sender, EventArgs e)
        {
            if (ListBox.SelectedItem == null && ListBox.Items.Count > 0)
                ListBox.SelectedIndex = 0;
        }

        protected override void SetValuePart(TItem valuePart)
        {
            ListBox.Items.Add(valuePart);
            EnableControls();
        }

        protected override void GrabList(TList list)
        {
            list.Clear();

            foreach (object item in ListBox.Items)
            {
                TItem newItem = new();
                newItem.CopyFrom((TItem)item);
                list.Add(newItem);
            }
        }

        protected override Control GetControl() => ListBox;

        protected override void ClearValue()
        {
            ListBox.ClearSelected();
            ListBox.Items.Clear();
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
            SetControlBackColor(ListBox, Colors.Lighter(7));

            if (Buttons != null)
                foreach (OxIconButton button in Buttons)
                    button.BaseColor = BaseColor;
        }

        public GetMaximumCount? OnGetMaximumCount;

        protected virtual int MaximumItemsCount => -1;

        public ListItemsControl() : base() { }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            RecalcEditButtonVisible();
        }

        private void RecalcEditButtonVisible()
        {
            if (ButtonsPanel == null)
                return;

            SetEditButtonVisible(!readOnly && (CalcedButtonsHeight <= ButtonsPanel.Height));
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
    }
}