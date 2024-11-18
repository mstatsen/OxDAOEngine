using OxLibrary;
using OxLibrary.Controls;
using OxLibrary.Panels;
using OxDAOEngine.ControlFactory;
using OxDAOEngine.Data;

namespace OxDAOEngine.View
{
    public abstract class ItemCard<TField, TDAO, TFieldGroup>
        : OxCard, IItemCard<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
        where TFieldGroup : notnull, Enum
    {
        protected virtual int CardHeight => 240;
        protected virtual int CardWidth => 440;

        public ItemCard(ItemViewMode viewMode) : base()
        {
            ViewMode = viewMode;
            EditorButton.SetContentSize(25, 20);
            EditorButton.Click += (s, e) => DataManager.EditItem<TField, TDAO>(item);
            EditorButton.Visible = ViewMode == ItemViewMode.WithEditLink;
            Header.AddToolButton(EditorButton);
            Builder = DataManager.Builder<TField, TDAO>(ControlScope.CardView, true);
            Layouter = Builder.Layouter;
            Margins.SetSize(OxSize.Extra);
            Paddings.SetSize(OxSize.Large);
            HeaderHeight = 28;
            SetContentSize(CardWidth, CardHeight);
            Icon = DataManager.ListController<TField, TDAO>().Icon;
        }

        public override Color DefaultColor => EngineStyles.CardColor;
        private readonly OxIconButton EditorButton = new(OxIcons.Pencil, 20);

        protected override void PrepareColors()
        {
            base.PrepareColors();
            Header.Label.ForeColor = fontColors.BaseColor;
        }

        private void LayoutControls()
        {
            Layouter.LayoutControls();
            AlignControls();
            AfterLayoutControls();
        }

        protected virtual void AfterLayoutControls() { }

        protected virtual void AlignControls() { }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (IsControlsReady)
                AfterLayoutControls();
        }

        protected void ClearLayoutTemplate()
        {
            Layouter.Template.Parent = this;
            Layouter.Template.Left = 0;
            Layouter.Template.Top = 0;
            Layouter.Template.CaptionVariant = ControlCaptionVariant.Left;
            Layouter.Template.WrapLabel = true;
            Layouter.Template.MaximumLabelWidth = 80;
            Layouter.Template.BackColor = Color.Transparent;
            Layouter.Template.FontColor = fontColors.BaseColor;
            Layouter.Template.FontStyle = FontStyle.Bold;
            Layouter.Template.LabelColor = fontColors.Lighter();
            Layouter.Template.LabelStyle = FontStyle.Italic;
            Layouter.Template.AutoSize = true;
        }

        protected abstract void PrepareLayouts();

        private void PrepareLayoutsInternal()
        {
            ClearLayoutTemplate();
            PrepareLayouts();
        }

        private void ItemChangeHandler(object sender, DAOEntityEventArgs e) =>
            PrepareControls();

        protected void SetColors()
        {
            if (item is null)
                return;
            
            ItemColorer<TField, TDAO> itemColorer = DataManager.ControlFactory<TField, TDAO>().ItemColorer;
            BaseColor = itemColorer.BaseColor(item);
            fontColors.BaseColor = itemColorer.ForeColor(item);
        }

        private bool IsControlsReady = false;
        private void PrepareControls()
        {
            IsControlsReady = false;

            try
            {
                SetColors();
                SetTitle();
                FillControls();
                ClearLayoutsInternal();
                PrepareLayoutsInternal();
                LayoutControls();
            }
            finally
            {
                IsControlsReady = true;
            }
        }

        private void ClearLayoutsInternal()
        {
            ClearLayouts();
            Layouter?.Clear();
        }

        protected virtual void ClearLayouts() { }

        private void FillControls()
        {
            if (item is not null)
                Builder.FillControls(item);
        }

        private void SetTitle() =>
            Text = GetTitle();

        protected virtual string? GetTitle() =>
            item?.ToString();

        public void ApplySettings() { }

        private TDAO? item;

        public TDAO? Item
        {
            get => item;
            set
            {
                if (item is not null)
                    item.ChangeHandler -= ItemChangeHandler;

                item = value;

                if (item is not null)
                    item.ChangeHandler += ItemChangeHandler;

                PrepareControls();
                PrepareColors();
                SetTitle();
            }
        }

        private readonly ItemViewMode ViewMode;

        public OxPane AsPane => this;

        protected readonly ControlBuilder<TField, TDAO> Builder;
        protected readonly ControlLayouter<TField, TDAO> Layouter;
        private readonly OxColorHelper fontColors = new(default);
    }
}