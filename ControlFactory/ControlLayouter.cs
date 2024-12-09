using OxLibrary.Controls;
using OxLibrary.Geometry;
using OxDAOEngine.ControlFactory.Accessors;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Fields;
using OxDAOEngine.Data.Types;

namespace OxDAOEngine.ControlFactory;

public class ControlLayouter<TField, TDAO>
    where TField : notnull, Enum
    where TDAO : RootDAO<TField>, new()
{
    public readonly ControlLayouts<TField> Layouts = new();
    public ControlBuilder<TField, TDAO> Builder;

    private readonly Dictionary<TField, PlacedControl<TField>> PlacedControls = new();
    private readonly Dictionary<OxLabel, TField> PlacedLabels = new();

    public PlacedControl<TField>? PlacedControl(TField field)
    {
        PlacedControls.TryGetValue(field, out var control);
        return control;
    }

    public void Clear()
    {
        foreach (PlacedControl<TField> placedControl in PlacedControls.Values)
            placedControl.DetachParent();

        PlacedControls.Clear();
        PlacedLabels.Clear();
        Layouts.Clear();
    }

    public ControlLayouter(ControlBuilder<TField, TDAO> builder) => 
        Builder = builder;

    private void LayoutControl(ControlLayout<TField> layout)
    {
        layout.SupportClickedLabels =
            TypeHelper.Helper<ControlScopeHelper>().SupportClickedLabels(Builder.Scope) &&
            FieldSupportLabelClick(layout.Field);
        TField field = layout.Field;

        if (PlacedControls.TryGetValue(field, out var placedControl))
            layout.ApplyLayout(placedControl);
        else
        {
            ControlAccessor<TField, TDAO> controlAccessor = (ControlAccessor<TField, TDAO>)Builder[field];
            placedControl = controlAccessor.LayoutControl(layout);
            PlacedControls.Add(field, placedControl);

            if (placedControl.Label is not null)
                PlacedLabels.Add(placedControl.Label, layout.Field);
        }

        if (FieldSupportLabelClick(layout.Field))
            SetLabelClickHander(placedControl);
    }

    private bool FieldSupportLabelClick(TField field) =>
        !Builder.Context(field).IsView
        || fieldHelper is null 
        || fieldHelper.GetFieldType(field) is not FieldType.List;

    private void SetLabelClickHander(PlacedControl<TField> placedControl)
    {
        OxLabel? label = placedControl?.Label;

        if (label is null)
            return;

        label.Click -= ExtractLabelClick;
        label.Click += ExtractLabelClick;
    }

    private readonly FieldHelper<TField> fieldHelper = TypeHelper.FieldHelper<TField>();

    private void ExtractLabelClick(object? sender, EventArgs e)
    {
        OxLabel? label = (OxLabel?)sender;

        if (label is null 
            || !PlacedLabels.TryGetValue(label, out TField? field))
            return;

        object? value = Builder.ObjectValue(field);

        if (fieldHelper.GetFieldType(field) is FieldType.List
            && value is string)
            return;

        if (fieldHelper.GetFieldType(field) is FieldType.Enum
            && value is string stringValue)
        {
            ITypeHelper? helper = fieldHelper.GetHelper(field);

            if (helper is not null)
                value = helper.Parse(stringValue);
        }

        DataManager.ViewItems<TField, TDAO>(field, value);
    }

    public ControlLayout<TField> Template => Layouts.Template;

    public int Count => Layouts.Count;

    public ControlLayout<TField>? Last =>
        Layouts.Last;

    public void LayoutControls()
    {
        foreach (ControlLayout<TField> layout in Layouts)
            LayoutControl(layout);
    }

    private short GetMinimumLabelLeft(List<ControlLayout<TField>> layouts)
    {
        short minimum = short.MaxValue;

        foreach (ControlLayout<TField> layout in layouts)
        { 
            if (layout is null 
                || layout.CaptionVariant.Equals(ControlCaptionVariant.None) 
                || !PlacedControls.TryGetValue(layout.Field, out var placedControl))
                continue;

            minimum = OxSH.Min(minimum, placedControl.LabelLeft);
        }

        return minimum;
    }

    private void SetLabelsLeft(List<ControlLayout<TField>> layouts, short left)
    {
        if (layouts is null)
            return;

        foreach (ControlLayout<TField> layout in layouts)
            if (layout is not null)
                PlacedControls[layout.Field].LabelLeft = left;
    }

    public void AlignLabels(ControlLayouts<TField> layouts, short moveControlColserBy = 0)
    {
        SetLabelsLeft(layouts, GetMinimumLabelLeft(layouts));

        if (moveControlColserBy > 0)
            MovePlacedControlsToLeft(layouts, moveControlColserBy);
    }

    public void AlignLabels(List<TField> fields, short moveControlColserBy = 0)
    {
        ControlLayouts<TField> layouts = new();

        foreach (TField field in fields)
        {
            ControlLayout<TField>? layout = Layouts[field];

            if (layout is not null)
                layouts.Add(layout);
        }

        AlignLabels(layouts, moveControlColserBy);
    }

    public ControlLayout<TField> AddFromTemplate(TField field, bool autoOffset = false, bool offsetWithMargins = true) =>
        Layouts.AddFromTemplate(field, autoOffset, offsetWithMargins);

    public ControlLayout<TField> AddFromTemplate(TField field, int verticalOffset) =>
        Layouts.AddFromTemplate(field, verticalOffset);

    public ControlLayout<TField>? this[TField field] => Layouts[field];

    public void MovePlacedControlsToLeft(ControlLayouts<TField> layouts, short offset)
    {
        foreach (TField field in layouts.Fields)
        {
            PlacedControl<TField>? placedControl = PlacedControl(field);

            if (placedControl is not null)
                placedControl.Control.Left -= offset;
        }
    }
}