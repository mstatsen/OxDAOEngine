using OxLibrary;
using OxLibrary.Controls;
using OxLibrary.Dialogs;
using OxDAOEngine.ControlFactory.Accessors;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Types;

namespace OxDAOEngine.ControlFactory.BatchUpdate
{
    public class BatchUpdatePanel<TField, TDAO> : OxDialogMainPanel
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public GetListEvent<TField, TDAO>? ItemsGetter;

        public EventHandler? BatchUpdateCompleted;

        private readonly string ItemsCaption = DataManager.ListController<TField, TDAO>().ListName;

        public void SetItemsCount() => 
            countLabel.Text = $"Selected {ItemsCaption}: {(ItemsGetter != null ? ItemsGetter().Count.ToString() : "N/A")}";

        public void UpdateItems()
        {
            if (ItemsGetter == null)
                return;

            if (FieldIsEmpty)
                return;

            foreach (TDAO item in ItemsGetter())
                item.StartSilentChange();

            foreach (TDAO item in ItemsGetter())
                item[FieldAccessor.EnumValue] = ValueAccessor.Value;

            foreach (TDAO item in ItemsGetter())
            {
                TDAO? findItem = DataManager.FullItemsList<TField, TDAO>().Find(i => i.Equals(item));

                if (findItem != null)
                    findItem.Modified = true;

                item.FinishSilentChange();
            }

            BatchUpdateCompleted?.Invoke(this, EventArgs.Empty);
        }

        public override Color DefaultColor => EngineStyles.BatchUpdateColor;
        public const string BatchUpdateTitle = "Batch Update";

        public BatchUpdatePanel(OxForm form) : base(form)
        {
            controlBuilder = DataManager.Builder<TField, TDAO>(ControlScope.BatchUpdate);
            countLabel.Parent = this;
            countLabel.Top = ContentContainer.Height - 30;
            ControlPainter.ColorizeControl(countLabel, BaseColor);
            FieldAccessor = (FieldAccessor<TField, TDAO>)controlBuilder[TypeHelper.FieldHelper<TField>().FieldMetaData];
            PrepareFieldAccessor();
            Text = BatchUpdateTitle;
            SetContentSize(360, 120);
        }

        public bool FieldIsEmpty =>
            FieldAccessor.Value == null 
            || FieldAccessor.Value is NullObject;


        private void EnabledOKButton() =>
            buttonsDictionary[OxDialogButton.OK].Enabled = !FieldIsEmpty;

        private void PrepareFieldAccessor()
        {
            FieldAccessor.ValueChangeHandler += FieldChangeHandler;
            PlacedControl<TField> PlacedFieldsControl = LayoutFieldControl();
            ControlPainter.ColorizeControl(PlacedFieldsControl.Control, BaseColor);
            valueLabel.Parent = this;
            valueLabel.Top = PlacedFieldsControl.Control.Bottom + 16;
            valueLabel.Left = PlacedFieldsControl.LabelLeft;
            ControlPainter.ColorizeControl(valueLabel, BaseColor);

            if (PlacedFieldsControl.Label != null)
                PlacedFieldsControl.Label.ForeColor = valueLabel.ForeColor;

            CreateValueLayout(PlacedFieldsControl.Layout);
            EnabledOKButton();
        }

        private void CreateValueLayout(ControlLayout<TField> template)
        {
            ValueLayout.CopyFrom(template);
            ValueLayout.BackColor = Colors.Lighter(6);
            ValueLayout.Top = FieldAccessor.Bottom + 8;
            ValueLayout.CaptionVariant = ControlCaptionVariant.None;
        }

        private PlacedControl<TField> LayoutFieldControl()
        {
            ControlLayout<TField> fieldLayout = new()
            {
                Field = TypeHelper.FieldHelper<TField>().FieldMetaData,
                Parent = this,
                Left = 50,
                Top = 12,
                Height = 26,
                Anchors = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right,
            };

            fieldLayout.Width = SavedWidth - fieldLayout.Left - 16;
            return FieldAccessor.LayoutControl(fieldLayout);
        }

        private void FieldChangeHandler(object? sender, EventArgs e)
        {
            HideValueControl();
            EnabledOKButton();

            if (FieldIsEmpty)
                return;

            ValueAccessor = (ControlAccessor<TField, TDAO>)controlBuilder[FieldAccessor.EnumValue];

            if (ValueAccessor == null)
                return;

            LayoutValueControl();
            SetFirstGameValue();
        }

        private void SetFirstGameValue()
        {
            TDAO? firstItem = ItemsGetter?.Invoke().First;

            if (firstItem != null)
                ValueAccessor.Value = firstItem[FieldAccessor.EnumValue];
        }

        private void LayoutValueControl()
        {
            ValueLayout.Field = FieldAccessor.EnumValue;
            CurrentValueControl = ValueAccessor.LayoutControl(ValueLayout);
            CurrentValueControl.Control.Width =
                CurrentValueControl.Control is OxSpinEdit
                ? 80
                : FieldAccessor.Width;
            ControlPainter.ColorizeControl(CurrentValueControl.Control, BaseColor);
            OxControlHelper.AlignByBaseLine(CurrentValueControl.Control, valueLabel);
            valueLabel.Visible = true;
        }

        private void HideValueControl()
        {
            if (CurrentValueControl == null)
                return;

            ValueAccessor.Control.Visible = false;
            valueLabel.Visible = false;
        }

        private readonly FieldAccessor<TField, TDAO> FieldAccessor = default!;
        private ControlAccessor<TField, TDAO> ValueAccessor = default!;
        private PlacedControl<TField>? CurrentValueControl;
        private readonly ControlBuilder<TField, TDAO> controlBuilder;
        private readonly ControlLayout<TField> ValueLayout = new();

        private readonly OxLabel valueLabel = new()
        {
            Text = "Value",
            AutoSize = true,
            Visible = false,
            Font = EngineStyles.DefaultFont
        };

        private readonly OxLabel countLabel = new()
        {
            Text = "Selected Items: ",
            Anchor = AnchorStyles.Left | AnchorStyles.Bottom,
            Left = 24,
            AutoSize = true,
            Visible = true,
            Font = EngineStyles.DefaultFont
        };
    }
}