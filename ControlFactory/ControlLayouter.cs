using OxLibrary.Controls;
using OxDAOEngine.ControlFactory.Accessors;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Fields;
using OxDAOEngine.Data.Types;

namespace OxDAOEngine.ControlFactory
{
    public class ControlLayouter<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public readonly ControlLayouts<TField> Layouts = new();
        public ControlBuilder<TField, TDAO> Builder;

        private readonly Dictionary<TField, PlacedControl<TField>> PlacedControls = new();

        public PlacedControl<TField>? PlacedControl(TField field)
        {
            PlacedControls.TryGetValue(field, out var control);
            return control;
        }

        public void Clear()
        {
            foreach (PlacedControl<TField> placedControl in PlacedControls.Values)
                placedControl.DetachParent();

            Layouts.Clear();
        }

        public ControlLayouter(ControlBuilder<TField, TDAO> builder) => 
            Builder = builder;

        private void LayoutControl(ControlLayout<TField> layout)
        {
            layout.SupportClickedLabels = 
                ControlScopeHelper.SupportClickedLabels(Builder.Scope);
            TField field = layout.Field;

            if (PlacedControls.TryGetValue(field, out var placedControl))
                layout.ApplyLayout(placedControl);
            else
            {
                ControlAccessor<TField, TDAO> controlAccessor = (ControlAccessor<TField, TDAO>)Builder[field];
                placedControl = controlAccessor.LayoutControl(layout);
                PlacedControls.Add(field, placedControl);
            }

            SetLabelClickHander(placedControl);
        }

        private void SetLabelClickHander(PlacedControl<TField> placedControl)
        {
            OxLabel? label = placedControl?.Label;

            if (label == null)
                return;

            label.Click -= ExtractLabelClick;
            label.Click += ExtractLabelClick;
        }

        private void ExtractLabelClick(object? sender, EventArgs e)
        {
            OxLabel? label = (OxLabel?)sender;

            if (label == null)
                return;

            //TODO: replace foreach with dictionary?
            foreach (var item in PlacedControls)
            {
                if (item.Value.Label != label)
                    continue;

                object? value = Builder.Value(item.Key);
                FieldHelper<TField> fieldHelper = TypeHelper.FieldHelper<TField>();

                if (fieldHelper.GetFieldType(item.Key) == FieldType.Enum
                    && value is string stringValue)
                {
                    ITypeHelper? helper = fieldHelper.GetHelper(item.Key);

                    if (helper != null)
                        value = helper.Parse(stringValue);
                }

                DataManager.ViewItems<TField, TDAO>(item.Key, value);
                break;
            }
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

        private int GetMinimumLabelLeft(List<ControlLayout<TField>> layouts)
        {
            int minimum = int.MaxValue;

            foreach (ControlLayout<TField> layout in layouts)
            { 
                if (layout == null || 
                    layout.CaptionVariant == ControlCaptionVariant.None ||
                    !PlacedControls.TryGetValue(layout.Field, out var placedControl))
                    continue;

                minimum = Math.Min(minimum, placedControl.LabelLeft);
            }

            return minimum;
        }

        private void SetLabelsLeft(List<ControlLayout<TField>> layouts, int left)
        {
            if (layouts == null)
                return;

            foreach (ControlLayout<TField> layout in layouts)
                if (layout != null)
                    PlacedControls[layout.Field].LabelLeft = left;
        }

        public void AlignLabels(ControlLayouts<TField> layouts) => 
            SetLabelsLeft(layouts, GetMinimumLabelLeft(layouts));

        public void AlignLabels(List<TField> fields)
        {
            ControlLayouts<TField> layouts = new();

            foreach (TField field in fields)
            {
                ControlLayout<TField>? layout = Layouts[field];

                if (layout != null)
                    layouts.Add(layout);
            }

            AlignLabels(layouts);
        }

        public ControlLayout<TField> AddFromTemplate(TField field, bool autoOffset = false, bool offsetWithMargins = true) =>
            Layouts.AddFromTemplate(field, autoOffset, offsetWithMargins);

        public ControlLayout<TField> AddFromTemplate(TField field, int verticalOffset) =>
            Layouts.AddFromTemplate(field, verticalOffset);

        public ControlLayout<TField>? this[TField field] => Layouts[field];
    }
}