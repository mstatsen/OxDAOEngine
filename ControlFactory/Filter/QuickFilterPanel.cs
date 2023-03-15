using OxLibrary;
using OxLibrary.Controls;
using OxXMLEngine.ControlFactory.Accessors;
using OxXMLEngine.Data;
using OxXMLEngine.Data.Fields;
using OxXMLEngine.Data.Filter;
using OxXMLEngine.Data.Types;
using OxXMLEngine.Settings;

namespace OxXMLEngine.ControlFactory.Filter
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

            if (Variant != QuickFilterVariant.Base)
                ShowSettingsButton = false;

            Layouter = Builder.Layouter;
            SetTitle();
            Waiter = new OxWaiter(InvokeChangeHandler);
        }

        public bool OnlyText => Layouter.Count == 1;

        public void RenewFilterControls()
        {
            if (Builder == null)
                return;

            LayoutControls();

            List<TField>? fieldList = fieldHelper.FullList(FieldsVariant.QuickFilter);

            if (fieldList != null)
                foreach (TField field in fieldList)
                    Builder[field].RenewControl();

            RecolorControls();
        }

        public void SetFilter(IFieldMapping<TField>? filter)
        {
            try
            {
                if (Builder == null)
                    return;

                SetChangeHandlers(false);

                if (filter == null)
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
            QuickFilterLayouter == null ? TextFields[0] : QuickFilterLayouter.TextFilterContainer;

        private List<TField> TextFields
        {
            get
            {
                List<TField> list = Settings.QuickFilterTextFields.Fields;
                List<TField>? textFields = fieldHelper.GetFields(FieldsVariant.QuickFilterText, FieldsFilling.Min);

                if (list.Count == 0 && textFields != null)
                    list.AddRange(textFields);

                return list;
            }
        }

        public Filter<TField, TDAO>? ActiveFilter
        {
            get
            {
                if (Builder == null)
                    return null;

                FilterRules<TField> rules = new();

                foreach (TField field in QuickFilterFields)
                    rules.Add(field);

                SimpleFilter<TField, TDAO> basePart = new();
                Builder.GrabControls(basePart, rules);

                SimpleFilter<TField, TDAO> textFilter = new(FilterConcat.OR);

                FilterOperation textFilterOperation = TypeHelper.Helper<TextFilterOperationHelper>()
                    .Operation(Settings.QuickFilterTextFieldOperation);

                foreach (TField field in TextFields)
                    textFilter.AddFilter(field, textFilterOperation, Builder.Value(TextFilterContainer));

                Filter<TField, TDAO> activeFilter = new();
                activeFilter.AddFilter(basePart, FilterConcat.AND);
                activeFilter.AddFilter(textFilter, FilterConcat.AND);
                return activeFilter;
            }
            set
            {
                if (value == null)
                    return;

                foreach (TField field in QuickFilterFields)
                    foreach (FilterGroups<TField, TDAO> groups in value.Root)
                        foreach (FilterGroup<TField, TDAO> group in groups)
                            foreach (SimpleFilter<TField, TDAO> simpleFilter in group)
                                if (simpleFilter.Rules.Contains(r => r.Field.Equals(field)))
                                {
                                    object? filteringValue = fieldHelper.IsCalcedField(field)
                                        ? simpleFilter.CalcedValues.ContainsKey(field) 
                                            ? simpleFilter.CalcedValues[field]
                                            : null
                                        : filteringValue = simpleFilter[field];

                                    if (TypeHelper.FieldIsTypeHelpered(field))
                                        filteringValue = TypeHelper.TypeObject(filteringValue);

                                    Builder[field].Value = filteringValue;
                                }
            }
        }

        public void ClearControls()
        {
            updating = true;

            try
            {
                List<TField>? fields = fieldHelper.FullList(FieldsVariant.QuickFilter);

                if (fields != null)
                    foreach (TField field in fields)
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

        protected override void PrepareColors()
        {
            base.PrepareColors();

            if (Builder != null)
                RecolorControls();
        }

        protected override void Dispose(bool disposing)
        {
            if (Waiter != null && Waiter.Enabled)
                Waiter.Stop();

            base.Dispose(disposing);
        }

        protected int FieldWidth(TField field) => 
            QuickFilterLayouter == null ? 100 : QuickFilterLayouter.FieldWidth(field);

        private bool NeedStartNewColumn(int columnControlCount) => 
            Variant switch
            {
                QuickFilterVariant.Export => 
                    columnControlCount == 6,
                QuickFilterVariant.Select => 
                    true,
                _ => 
                    columnControlCount == 3,
            };

        private void PrepareLayouts()
        {
            Layouter.Template.Top =
                Variant == QuickFilterVariant.Select
                || Variant == QuickFilterVariant.Export
                ? 4
                : 0;
            Layouter.Template.Left = FirstControlLeft;
            Layouter.Template.Height = 22;
            Layouter.Template.Parent = this;
            Layouter.Template.BackColor = BackColor;
            Layouter.Template.FontColor = ForeColor;
            Layouter.Template.LabelColor = ForeColor;

            bool newColumn;
            bool needVerticalOffset;
            int layoutLeft;
            int layoutTop;
            int columnControlCount = 0;
            int maxColumnWidth = 0;
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
                layoutTop = -1;

                if (Layouter.Count > 0)
                {
                    newColumn = NeedStartNewColumn(columnControlCount);
                    needVerticalOffset = !newColumn;
                    layoutLeft = Layouter.Template.Left;

                    ControlLayout<TField>? lastLayout = Layouter.Last;

                    if (QuickFilterLayouter != null &&
                        lastLayout != null &&
                        QuickFilterLayouter.IsLastLayoutForOneRow(field, lastLayout.Field))
                    {
                        newColumn = false;
                        captionVariant = ControlCaptionVariant.None;
                        layoutLeft = lastLayout.Right + 4;
                        needVerticalOffset = false;
                        columnControlCount--;
                        layoutTop = lastLayout.Top;
                    }

                    if (newColumn)
                    {
                        Layouter.Template.Left = maxColumnWidth + 84;
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

                if (layoutTop > -1)
                    layout.Top = layoutTop;

                layout.Width = FieldWidth(field);
                layout.CaptionVariant = captionVariant;
                maxColumnWidth = Math.Max(maxColumnWidth, layout.Right);
            }

            ControlLayout<TField> layoutTextFilter = Layouter.AddFromTemplate(TextFilterContainer);

            if (layoutForOffsetText != null)
                layoutTextFilter.OffsetVertical(layoutForOffsetText);

            if (Layouter.Count == 1)
            {
                layoutTextFilter.CaptionVariant = ControlCaptionVariant.None;
                layoutTextFilter.Dock = DockStyle.Fill;
            }
            else
            {
                layoutTextFilter.CaptionVariant = ControlCaptionVariant.Left;
                layoutTextFilter.Left = 50;
                layoutTextFilter.Height = 26;
                layoutTextFilter.Width = Width - layoutTextFilter.Left - 10;
                layoutTextFilter.Anchors = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            }
        }

        public void RecalcPaddings() =>
            Paddings.SetSize(OnlyText ? OxSize.None : OxSize.Medium);

        private int FirstControlLeft =>
            Variant == QuickFilterVariant.Export
                ? 84 
                : 60;

        private void FilterControlsChange(object? sender, EventArgs e)
        {
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

        private void ClearButtonClick(object? sender, EventArgs e) =>
            ClearControls();

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

            if (FilterTextControl == null)
                return;

            int calcedWidth = FilterTextControl.Control.Right + 10;
            int calcedHeight = Layouter.Count == 1 ? 40 : FilterTextControl.Control.Bottom;
            SetTextFilterBorder();
            calcedHeight += Paddings.Bottom;
            SetContentSize(calcedWidth, calcedHeight);
            Width = calcedWidth;

            if (FilterTextControl.Label != null)
            {
                FilterTextControl.Label.Text =
                    TextFields.Count == 1
                    ? TypeHelper.Name(TextFilterContainer)
                    : Consts.QuickFilterTextFieldCaption;
                FilterTextControl.RecalcLabel();
            }

            int delta;

            if (QuickFilterFields.Count > 0)
            {
                OxLabel? temlpateLabel = Layouter.PlacedControl(QuickFilterFields[0])?.Label;
                OxLabel? textControlLabel = FilterTextControl.Label;

                if (temlpateLabel != null && textControlLabel != null)
                    textControlLabel.Left = textControlLabel.Left < 0 
                        ? temlpateLabel.Left 
                        : temlpateLabel.Right - textControlLabel.Width;

                foreach (TField field in QuickFilterFields)
                {
                    PlacedControl<TField>? placedControl = Layouter.PlacedControl(field);

                    if (placedControl == null ||
                        placedControl.Label == null)
                        continue;

                    string caption = FieldCaption(field);

                    if (caption != placedControl.Label.Text)
                    {
                        placedControl.Label.Text = FieldCaption(field);
                        placedControl.RecalcLabel();
                    }
                }

                if (temlpateLabel != null)
                {
                    delta = temlpateLabel.Left - 4;

                    if (delta < 0)
                    {
                        foreach (TField field in QuickFilterFields)
                        {
                            PlacedControl<TField>? placedControl = Layouter.PlacedControl(field);

                            if (placedControl == null)
                                continue;

                            placedControl.Control.Left -= delta;

                            if (placedControl.Label != null)
                                placedControl.Label.Left -= delta;
                        }

                        calcedWidth += delta;
                    }
                }

                calcedWidth += 8;
                SetContentSize(calcedWidth, calcedHeight);
            }
        }

        private void SetTextFilterBorder()
        {
            PlacedControl<TField>? textFilterControl = Layouter.PlacedControl(TextFilterContainer);

            if (textFilterControl == null)
                return;

            ((OxTextBox)textFilterControl.Control).BorderStyle = 
                Layouter.Count == 1 
                    ? BorderStyle.None 
                    : BorderStyle.FixedSingle;
        }

        private string FieldCaption(TField field)
        {
            string caption = string.Empty;

            if (QuickFilterLayouter != null)
                caption = QuickFilterLayouter.FieldCaption(field, Variant);

            PlacedControl<TField>? placedControl = Layouter.PlacedControl(field);

            if (caption == string.Empty && 
                placedControl != null && 
                placedControl.Label != null)
            {
                OxLabel label = placedControl.Label;

                if (label != null)
                    caption = label.Text;
            }

            return caption;
        }

        private void SetChangeHandlers(TField field, bool set)
        {
            if (Builder == null)
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

        protected override void PrepareInnerControls()
        {
            base.PrepareInnerControls();
            ClearButton.SetContentSize(80, 23);
            ClearButton.Click += ClearButtonClick;
            Header.AddToolButton(ClearButton);
        }

        protected override SettingsPart SettingsPart => SettingsPart.QuickFilter;

        private const string QuickFilterTitle = "Quick Filter";
        private readonly ControlBuilder<TField, TDAO> Builder;
        private readonly ControlLayouter<TField, TDAO> Layouter;
        private readonly OxButton ClearButton = new ("Clear", OxIcons.eraser)
        {
            Font = new Font(Styles.FontFamily, Styles.DefaultFontSize - 1, FontStyle.Bold)
        };
        private bool updating = false;
        private readonly OxWaiter Waiter;

        public QuickFilterVariant Variant { get; private set; }

        protected override void ApplySettingsInternal()
        {
            if (Observer.QuickFilterFieldsChanged || Observer.QuickFilterTextFieldsChanged)
                RenewFilterControls();

            ActiveFilter = Settings.Filter;
        }

        public override void SaveSettings()
        {
            base.SaveSettings();
            Settings.Filter = ActiveFilter;
        }

        protected List<TField> QuickFilterFields => 
            Variant switch
            {
                QuickFilterVariant.Select => fieldHelper.SelectQuickFilterFields,
                _ => Settings.QuickFilterFields.Fields,
            };


        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (Visible)
                RecolorControls();
        }

        private readonly FieldHelper<TField> fieldHelper = 
            TypeHelper.FieldHelper<TField>();
    }
}