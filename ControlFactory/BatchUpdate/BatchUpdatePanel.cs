﻿using OxLibrary;
using OxLibrary.Controls;
using OxLibrary.Forms;
using OxLibrary.Geometry;
using OxDAOEngine.ControlFactory.Accessors;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Types;

namespace OxDAOEngine.ControlFactory.BatchUpdate;

public class BatchUpdatePanel<TField, TDAO> :
    OxDialogPanel
    where TField : notnull, Enum
    where TDAO : RootDAO<TField>, new()
{
    public GetListEvent<TField, TDAO>? ItemsGetter;

    public EventHandler? BatchUpdateCompleted;

    private readonly string ItemsCaption = DataManager.ListController<TField, TDAO>().ListName;

    public void SetItemsCount() => 
        countLabel.Text = $"Selected {ItemsCaption}: {(ItemsGetter is not null ? ItemsGetter().Count.ToString() : "N/A")}";

    public void UpdateItems()
    {
        if (ItemsGetter is null || FieldIsEmpty)
            return;
        foreach (TDAO item in ItemsGetter())
            item.StartSilentChange();

        foreach (TDAO item in ItemsGetter())
            item[FieldAccessor.EnumValue] = ValueAccessor.Value;

        foreach (TDAO item in ItemsGetter())
        {
            TDAO? findItem = DataManager.FullItemsList<TField, TDAO>().Find(i => i.Equals(item));

            if (findItem is not null)
                findItem.Modified = true;

            item.FinishSilentChange();
        }

        BatchUpdateCompleted?.Invoke(this, EventArgs.Empty);
    }

    public override Color DefaultColor => EngineStyles.BatchUpdateColor;
    public const string BatchUpdateTitle = "Batch Update";

    public BatchUpdatePanel() : base()
    {
        Size = new(360, 120);
        controlBuilder = DataManager.Builder<TField, TDAO>(ControlScope.BatchUpdate);
        countLabel.Parent = this;
        countLabel.Top = OxSh.Sub(Height, 30);
        ControlPainter.ColorizeControl(countLabel, BaseColor);
        FieldAccessor = (FieldAccessor<TField, TDAO>)controlBuilder[TypeHelper.FieldHelper<TField>().FieldMetaData];
        PrepareFieldAccessor();
        Text = BatchUpdateTitle;
    }

    public bool FieldIsEmpty =>
        FieldAccessor.Value is null
        || (FieldAccessor.Value is IEmptyChecked ec && ec.IsEmpty);

    private void EnabledOKButton() =>
        Footer.SetButtonEnabled(OxDialogButton.OK, !FieldIsEmpty);

    private void PrepareFieldAccessor()
    {
        FieldAccessor.ValueChangeHandler += FieldChangeHandler;
        PlacedControl<TField> PlacedFieldsControl = LayoutFieldControl();
        ControlPainter.ColorizeControl(PlacedFieldsControl.Control, BaseColor);
        valueLabel.Parent = this;
        valueLabel.Top = OxSh.Add(PlacedFieldsControl.Control.Bottom, 16);
        valueLabel.Left = PlacedFieldsControl.LabelLeft;
        ControlPainter.ColorizeControl(valueLabel, BaseColor);

        if (PlacedFieldsControl.Label is not null)
            PlacedFieldsControl.Label.ForeColor = valueLabel.ForeColor;

        CreateValueLayout(PlacedFieldsControl.Layout);
        EnabledOKButton();
    }

    private void CreateValueLayout(ControlLayout<TField> template)
    {
        ValueLayout.CopyFrom(template);
        ValueLayout.BackColor = Colors.Lighter(6);
        ValueLayout.Top = OxSh.Add(FieldAccessor.Bottom, 8);
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

        fieldLayout.Width = OxSh.Sub(Width, fieldLayout.Left, 16);
        return FieldAccessor.LayoutControl(fieldLayout);
    }

    private void FieldChangeHandler(object? sender, EventArgs e)
    {
        HideValueControl();
        EnabledOKButton();

        if (FieldIsEmpty)
            return;

        ValueAccessor = (ControlAccessor<TField, TDAO>)controlBuilder[FieldAccessor.EnumValue];

        if (ValueAccessor is null)
            return;

        LayoutValueControl();
        SetFirstItemValue();
    }

    private void SetFirstItemValue()
    {
        TDAO? firstItem = ItemsGetter?.Invoke().First;

        if (firstItem is not null)
            ValueAccessor.Value = firstItem[FieldAccessor.EnumValue];
    }

    private void LayoutValueControl()
    {
        ValueLayout.Field = FieldAccessor.EnumValue;
        CurrentValueControl = ValueAccessor.LayoutControl(ValueLayout);
        CurrentValueControl.Control.Width =
            OxSh.Short(
                CurrentValueControl.Control is OxSpinEdit
                    ? 80
                    : FieldAccessor.Width
            );
        ControlPainter.ColorizeControl(CurrentValueControl.Control, BaseColor);
        OxControlHelper.AlignByBaseLine(CurrentValueControl.Control, valueLabel);
        valueLabel.Visible = OxB.T;
    }

    private void HideValueControl()
    {
        if (CurrentValueControl is null)
            return;

        ValueAccessor.Control.Visible = OxB.F;
        valueLabel.Visible = OxB.F;
    }

    private readonly FieldAccessor<TField, TDAO> FieldAccessor = default!;
    private ControlAccessor<TField, TDAO> ValueAccessor = default!;
    private PlacedControl<TField>? CurrentValueControl;
    private readonly ControlBuilder<TField, TDAO> controlBuilder;
    private readonly ControlLayout<TField> ValueLayout = new();

    private readonly OxLabel valueLabel = new()
    {
        Text = "Value",
        AutoSize = OxB.T,
        Visible = OxB.F,
        Font = OxStyles.DefaultFont
    };

    private readonly OxLabel countLabel = new()
    {
        Text = "Selected Items: ",
        Anchor = AnchorStyles.Left | AnchorStyles.Bottom,
        Left = 24,
        AutoSize = OxB.T,
        Visible = OxB.T,
        Font = OxStyles.DefaultFont
    };
}