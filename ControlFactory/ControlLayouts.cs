namespace OxDAOEngine.ControlFactory
{
    public class ControlLayouts<TField> : List<ControlLayout<TField>>
        where TField : notnull, Enum
    {
        public ControlLayout<TField> Template = new();

        private ControlLayout<TField> NewLayout(TField field) => new(field);

        public ControlLayout<TField> AddFromTemplate(TField field, int verticalOffset)
        {
            ControlLayout<TField> layout = NewLayout(field);
            layout.CopyFrom(Template);
            Add(layout, verticalOffset);
            return layout;
        }

        public virtual ControlLayout<TField> AddFromTemplate(TField field, 
            bool autoOffset = false, bool offsetWithMargins = true)
        {
            ControlLayout<TField> layout = NewLayout(field);
            layout.CopyFrom(Template);
            Add(layout, autoOffset, offsetWithMargins);
            return layout;
        }

        public List<TField> Fields
        {
            get
            {
                List<TField> result = new();

                foreach (ControlLayout<TField> layout in this)
                    result.Add(layout.Field);

                return result;
            }
        }

        public ControlLayout<TField> Add(ControlLayout<TField> layout, 
            bool autoOffset = false, bool offsetWithMargins = true)
        {
            if (autoOffset && Count > 0)
                layout.OffsetVertical(Last, offsetWithMargins);

            base.Add(layout);
            return layout;
        }

        public ControlLayout<TField> Add(ControlLayout<TField> layout, int offset)
        {
            layout.OffsetVertical(Last, offset);
            base.Add(layout);
            return layout;
        }

        public ControlLayout<TField>? this[TField field] => 
            Find(l => l.Field.Equals(field));

        public new void Clear()
        {
            Template.Clear();
            base.Clear();
        }

        public ControlLayout<TField>? Last =>
            Count > 0
                ? this[^1]
                : null;
    }
}