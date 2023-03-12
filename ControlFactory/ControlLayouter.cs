using OxXMLEngine.ControlFactory.Accessors;
using OxXMLEngine.Data;

namespace OxXMLEngine.ControlFactory
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
            if (PlacedControls.TryGetValue(layout.Field, out var placedControl))
                layout.ApplyLayout(placedControl);
            else
            {
                ControlAccessor<TField, TDAO> controlAccessor = (ControlAccessor<TField, TDAO>)Builder.Accessor(layout.Field);
                PlacedControls.Add(layout.Field, controlAccessor.LayoutControl(layout));
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
                if (layout == null)
                    continue;

                if (layout.CaptionVariant == ControlCaptionVariant.None)
                    continue;

                if (!PlacedControls.TryGetValue(layout.Field, out var placedControl))
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