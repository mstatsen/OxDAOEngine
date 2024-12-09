using OxLibrary;
using OxLibrary.Controls;
using OxLibrary.Handlers;
using OxLibrary.Waiter;
using OxDAOEngine.ControlFactory.Accessors;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Fields;
using OxDAOEngine.Data.Filter;
using OxDAOEngine.Data.Types;
using OxDAOEngine.Settings;
using OxDAOEngine.Data.Filter.Types;
using OxDAOEngine.Settings.Part;
using OxDAOEngine.Data.Fields.Types;

namespace OxDAOEngine.ControlFactory.Filter
{
    public partial class QuickFilterPanel<TField, TDAO> : FunctionsPanel<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        private readonly IQuickFilterLayouter<TField>? QuickFilterLayouter = 
            DataManager.ControlFactory<TField, TDAO>()?.CreateQuickFilterLayouter();

        public QuickFilterPanel(QuickFilterVariant variant)
        {
            Variant = variant;
            Builder = DataManager.Builder<TField, TDAO>(ControlScope.QuickFilter, false, Variant);

            if (Variant is not QuickFilterVariant.Base)
                SettingsAvailable = false;

            Layouter = Builder.Layouter;
            Icon = OxIcons.Filter;
            SetTitle();
            Waiter = new OxWaiter(InvokeChangeHandler);
        }

        public bool OnlyText => Layouter.Count is 1;

        public void RenewFilterControls()
        {
            if (Builder is null)
                return;

            LayoutControls();

            List<TField>? fieldList = fieldHelper.FullList(FieldsVariant.QuickFilter);

            if (fieldList is not null)
                foreach (TField field in fieldList)
                    Builder[field].RenewControl();

            RecolorControls();
        }

        public void SetFilter(IFieldMapping<TField>? filter)
        {
            try
            {
                if (Builder is null)
                    return;

                SetChangeHandlers(false);

                if (filter is null)
                {
                    ClearControls();
                    return;
                }

                updating = true;

                try
                {
                    foreach (TField field in QuickFilterFields)
                        Builder[field].Value = filter[field];

                    Builder[TextFilterContainer].Value = filter[TextFilterContainer];
                }
                finally
                {
                    updating = false;
                }
            }
            finally
            {
                SetChangeHandlers(true);
            }
        }

        private TField TextFilterContainer => 
            QuickFilterLayouter is null 
                ? TextFields[0] 
                : QuickFilterLayouter.TextFilterContainer;

        private List<TField> TextFields
        {
            get
            {
                List<TField> list = Settings.QuickFilterTextFields.Fields;
                List<TField>? textFields = fieldHelper.GetFieldsInternal(FieldsVariant.QuickFilterText, FieldsFilling.Min);

                if (list.Count is 0 
                    && textFields is not null)
                    list.AddRange(textFields);

                return list;
            }
        }

        public Filter<TField, TDAO>? activeFilter = null;

        public Filter<TField, TDAO>? ActiveFilter
        {
            get
            {
                if (Builder is null)
                    return null;

                if (activeFilter is null)
                    GrabActiveFilter();
                
                return activeFilter;
            }
            set
            {
                if (value is null)
                    return;

                foreach (FilterGroup<TField, TDAO> group in value)
                    foreach (FilterRule<TField> rule in group)
                        Builder[rule.Field].Value = rule.Value;
            }
        }

        private void GrabActiveFilter()
        {
            FilterGroup<TField, TDAO> basePart = new(FilterConcat.AND);
            Builder.GrabControls(basePart, QuickFilterFields);
            activeFilter = new(FilterConcat.AND)
            {
                basePart
            };

            if (!Builder[TextFilterContainer].IsEmpty)
            {
                FilterGroup<TField, TDAO> textPart = new(FilterConcat.OR);
                FilterOperation textFilterOperation = TypeHelper.Helper<TextFilterOperationHelper>()
                    .Operation(Settings.QuickFilterTextFieldOperation);

                foreach (TField field in TextFields)
                    textPart.Add(field, textFilterOperation, Builder.Value(TextFilterContainer));

                activeFilter.Add(textPart);
            }
        }

        public void ClearControls()
        {
            updating = true;

            try
            {
                foreach (TField field in fieldHelper.FullList(FieldsVariant.QuickFilter))
                    Builder[field].Clear();

                Builder[TextFilterContainer].Clear();
                RecolorControls();
            }
            finally
            {
                updating = false;
                InvokeChangeHandler();
            }
        }

        public event EventHandler? Changed;
        protected override Color FunctionColor => EngineStyles.QuickFilterColor;

        public override void PrepareColors()
        {
            base.PrepareColors();

            if (Builder is not null)
                RecolorControls();
        }

        protected override void Dispose(bool disposing)
        {
            if (Waiter is not null 
                && Waiter.Enabled)
                Waiter.Stop();

            base.Dispose(disposing);
        }

        protected short FieldWidth(TField field) =>
            (short)(QuickFilterLayouter is null
                ? 100
                : QuickFilterLayouter.FieldWidth(field));

        private bool NeedStartNewColumn(int columnControlCount) => 
            Variant switch
            {
                QuickFilterVariant.Export => 
                    columnControlCount is 6,
                QuickFilterVariant.Select => 
                    true,
                _ => 
                    columnControlCount is 3,
            };

        private void PrepareLayouts()
        {
            Layouter.Template.Top =
                (short)(Variant is QuickFilterVariant.Select
                        or QuickFilterVariant.Export
                ? 4
                : 0);
            Layouter.Template.Left = FirstControlLeft;
            Layouter.Template.Height = 22;
            Layouter.Template.Parent = this;
            Layouter.Template.BackColor = BackColor;
            Layouter.Template.FontColor = ForeColor;
            Layouter.Template.LabelColor = ForeColor;

            bool newColumn;
            bool needVerticalOffset;
            short layoutLeft;
            short layoutTop;
            int columnControlCount = 0;
            short maxColumnWidth = 0;
            ControlCaptionVariant captionVariant;
            ControlLayout<TField>? layoutForOffsetText = null;
            bool needSetLayoutForOffsetText = true;

            foreach (TField field in QuickFilterFields)
            {
                columnControlCount++;
                newColumn = false;
                layoutLeft = Layouter.Template.Left;
                captionVariant = ControlCaptionVariant.Left;
                needVerticalOffset = false;
                layoutTop = 0;
                bool layoutTopSet = false;

                if (Layouter.Count > 0)
                {
                    newColumn = NeedStartNewColumn(columnControlCount);
                    needVerticalOffset = !newColumn;
                    layoutLeft = Layouter.Template.Left;

                    ControlLayout<TField>? lastLayout = Layouter.Last;

                    if (QuickFilterLayouter is not null &&
                        lastLayout is not null 
                        && QuickFilterLayouter.IsLastLayoutForOneRow(field, lastLayout.Field))
                    {
                        newColumn = false;
                        captionVariant = ControlCaptionVariant.None;
                        layoutLeft = (short)(lastLayout.Right + 4);
                        needVerticalOffset = false;
                        columnControlCount--;
                        layoutTop = lastLayout.Top;
                        layoutTopSet = true;
                    }

                    if (newColumn)
                    {
                        Layouter.Template.Left = (short)(maxColumnWidth + 84);
                        layoutLeft = Layouter.Template.Left;
                        columnControlCount = 1;
                        maxColumnWidth = 0;
                    }
                }

                ControlLayout<TField> layout = Layouter.AddFromTemplate(field, needVerticalOffset);

                if (newColumn)
                    needSetLayoutForOffsetText = false;

                if (needSetLayoutForOffsetText)
                    layoutForOffsetText = layout;

                layout.Left = layoutLeft;

                if (layoutTopSet)
                    layout.Top = layoutTop;

                layout.Width = FieldWidth(field);
                layout.CaptionVariant = captionVariant;
                maxColumnWidth = Math.Max(maxColumnWidth, layout.Right);
            }

            ControlLayout<TField> layoutTextFilter = Layouter.AddFromTemplate(TextFilterContainer);

            if (layoutForOffsetText is not null)
                layoutTextFilter.OffsetVertical(layoutForOffsetText);

            if (Layouter.Count is 1)
            {
                layoutTextFilter.CaptionVariant = ControlCaptionVariant.None;
                layoutTextFilter.Dock = OxDock.Fill;
            }
            else
            {
                layoutTextFilter.CaptionVariant = ControlCaptionVariant.Left;
                layoutTextFilter.Left = 56;
                layoutTextFilter.Height = 26;
                layoutTextFilter.Width = (short)(Layouter.Last!.Right - layoutTextFilter.Left);
                layoutTextFilter.Anchors = AnchorStyles.Top | AnchorStyles.Left;
            }
        }

        public void RecalcPaddings() =>
            Padding.Size = (short)(OnlyText ? 0 : 2);

        private short FirstControlLeft =>
            (short)(Variant is QuickFilterVariant.Export
                ? 84
                : 60);

        private void FilterControlsChange(object? sender, EventArgs e)
        {
            activeFilter = null;
            RecolorControls();
            Waiter.Ready = false;

            if (!Waiter.Enabled)
                Waiter.Start();
        }

        private void RecolorControl(TField field, Color filledColor)
        {
            IControlAccessor accessor = Builder.Accessor(field);
            accessor.Control.BackColor =
                accessor.IsEmpty
                    ? BackColor
                    : filledColor;
        }

        private void RecolorControls()
        {
            Color filledColor = Colors.Lighter(5);

            foreach (TField field in QuickFilterFields)
                RecolorControl(field, filledColor);

            RecolorControl(TextFilterContainer, filledColor);
        }

        private int InvokeChangeHandler()
        {
            if (updating)
                return 1;

            if (!Created || Disposing)
                return 2;

            Waiter.Stop();

            BeginInvoke(Changed);
            return 0;
        }

        private void LayoutControls()
        {
            Layouter.Clear();
            PrepareLayouts();
            Layouter.LayoutControls();
            SetChangeHandlers(true);
            NormalizeControlsAndSize();
        }

        private void NormalizeControlsAndSize()
        {
            PlacedControl<TField>? FilterTextControl = Layouter.PlacedControl(TextFilterContainer);

            if (FilterTextControl is null)
                return;

            short calcedWidth = (short)(FilterTextControl.Control.Right + 10);
            short calcedHeight =
                (short)(Layouter.Count is 1
                    ? 40 
                    : FilterTextControl.Control.Bottom);
            SetTextFilterBorder();
            calcedHeight += Padding.Bottom;
            Size = new(calcedWidth, calcedHeight);
            Width = calcedWidth;

            if (FilterTextControl.Label is not null)
            {
                FilterTextControl.Label.Text =
                    TextFields.Count is 1
                        ? TypeHelper.Name(TextFilterContainer)
                        : Consts.QuickFilterTextFieldCaption;
                FilterTextControl.RecalcLabel();
            }

            int delta;

            if (QuickFilterFields.Count > 0)
            {
                OxLabel? temlpateLabel = Layouter.PlacedControl(QuickFilterFields[0])?.Label;
                OxLabel? textControlLabel = FilterTextControl.Label;

                if (temlpateLabel is not null 
                    && textControlLabel is not null)
                    textControlLabel.Left =
                        (short)(
                            textControlLabel.Left < 0
                                ? temlpateLabel.Left 
                                : temlpateLabel.Right - textControlLabel.Width
                        );

                foreach (TField field in QuickFilterFields)
                {
                    PlacedControl<TField>? placedControl = Layouter.PlacedControl(field);

                    if (placedControl is null 
                        || placedControl.Label is null)
                        continue;

                    string caption = FieldCaption(field);

                    if (!caption.Equals(placedControl.Label.Text))
                    {
                        placedControl.Label.Text = FieldCaption(field);
                        placedControl.RecalcLabel();
                    }
                }

                if (temlpateLabel is not null)
                {
                    delta = temlpateLabel.Left - 4;

                    if (delta < 0)
                    {
                        foreach (TField field in QuickFilterFields)
                        {
                            PlacedControl<TField>? placedControl = Layouter.PlacedControl(field);

                            if (placedControl is null)
                                continue;

                            placedControl.Control.Left -= delta;

                            if (placedControl.Label is not null)
                                placedControl.Label.Left -= (short)delta;
                        }

                        calcedWidth += (short)delta;
                    }
                }

                calcedWidth |= 8;
                Size = new(calcedWidth, calcedHeight);
            }
        }

        public override void OnSizeChanged(OxSizeChangedEventArgs e)
        {
            base.OnSizeChanged(e);

            if (!e.Changed 
                || QuickFilterFields.Count is 0
                || Layouter is null)
                return;

            PlacedControl<TField>? FilterTextControl = Layouter.PlacedControl(TextFilterContainer);

            if (FilterTextControl is not null)
                FilterTextControl.Control.Width =
                    Layouter[QuickFilterFields.Last()]!.Right
                    - FilterTextControl.Control.Left;
        }

        private void SetTextFilterBorder()
        {
            PlacedControl<TField>? textFilterControl = Layouter.PlacedControl(TextFilterContainer);

            if (textFilterControl is null)
                return;

            ((OxTextBox)textFilterControl.Control).BorderStyle = 
                Layouter.Count is 1 
                    ? BorderStyle.None 
                    : BorderStyle.FixedSingle;
        }

        private string FieldCaption(TField field)
        {
            string caption = string.Empty;

            if (QuickFilterLayouter is not null)
                caption = QuickFilterLayouter.FieldCaption(field, Variant);

            PlacedControl<TField>? placedControl = Layouter.PlacedControl(field);

            if (caption.Equals(string.Empty)
                && placedControl is not null 
                && placedControl.Label is not null)
            {
                OxLabel label = placedControl.Label;

                if (label is not null)
                    caption = label.Text;
            }

            return caption;
        }

        private void SetChangeHandlers(TField field, bool set)
        {
            if (Builder is null)
                return;

            if (set)
                Builder[field].ValueChangeHandler += FilterControlsChange;
            else Builder[field].ValueChangeHandler -= FilterControlsChange;
        }

        private void SetChangeHandlers(bool setHandlers)
        {
            if (setHandlers)
                SetChangeHandlers(false);

            foreach (TField field in QuickFilterFields)
                SetChangeHandlers(field, setHandlers);

            SetChangeHandlers(TextFilterContainer, setHandlers);
        }

        private void SetTitle() => 
            Text = Variant switch
            {
                QuickFilterVariant.Export => "Filter",
                _ => QuickFilterTitle,
            };

        protected override void PrepareInnerComponents()
        {
            base.PrepareInnerComponents();
            ClearButton.Size = new(80, 23);
            ClearButton.Click += (s, e) => ClearControls();
            Header.AddButton(ClearButton);
        }

        protected override SettingsPart SettingsPart => SettingsPart.QuickFilter;

        private const string QuickFilterTitle = "Quick Filter";
        private readonly ControlBuilder<TField, TDAO> Builder;
        private readonly ControlLayouter<TField, TDAO> Layouter;
        private readonly OxButton ClearButton = new ("Clear", OxIcons.Eraser)
        {
            Font = OxStyles.Font(-1, FontStyle.Bold)
        };
        private bool updating = false;
        private readonly OxWaiter Waiter;

        public QuickFilterVariant Variant { get; private set; }

        protected override void ApplySettingsInternal()
        {
            base.ApplySettingsInternal();

            if (Observer.QuickFilterFieldsChanged 
                || Observer.QuickFilterTextFieldsChanged)
                RenewFilterControls();

            ActiveFilter = Settings.Filter;
        }

        public override void SaveSettings()
        {
            base.SaveSettings();
            Settings.Filter = ActiveFilter;
            Settings.QuickFilterPinned = Pinned;
            Settings.QuickFilterExpanded = Expanded;
        }

        protected List<TField> QuickFilterFields => 
            Variant switch
            {
                QuickFilterVariant.Select => 
                    fieldHelper.SelectQuickFilterFields,
                _ => 
                    Settings.QuickFilterFields.Fields,
            };

        protected override DAOSetting VisibleSetting => DAOSetting.ShowQuickFilter;

        protected override DAOSetting PinnedSetting => DAOSetting.QuickFilterPinned;

        protected override DAOSetting ExpandedSetting => DAOSetting.QuickFilterExpanded;

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (Visible is not FunctionalPanelVisible.Hidden)
                RecolorControls();
        }

        private readonly FieldHelper<TField> fieldHelper = 
            TypeHelper.FieldHelper<TField>();
    }
}