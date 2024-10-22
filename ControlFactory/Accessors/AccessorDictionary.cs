using OxLibrary;
using OxLibrary.Controls;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Fields;
using OxDAOEngine.Data.Types;

namespace OxDAOEngine.ControlFactory.Accessors
{
    public class AccessorDictionary<TField, TDAO> : Dictionary<object, IControlAccessor>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public void AlignAccessors()
        {
            int maxLeft = 0;

            foreach (IControlAccessor accessor in Values)
                maxLeft = Math.Max(maxLeft, accessor.Left);

            foreach (IControlAccessor accessor in Values)
                accessor.Left = maxLeft;
        }

        public new void Clear()
        {
            ClearAccessorsParent();
            base.Clear();
        }

        private const int HorizontalSpace = 28;

        public IControlAccessor CreateAccessor(TField field, object key, Control parent, 
            string? caption, object value, Point location)
        {
            OxLabel captionLabel = new()
            {
                Parent = parent,
                Left = location.X,
                Text = caption,
                Font = Styles.Font(FontStyle.Italic),
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = true,
                Cursor = Cursors.Hand
            };
            captionLabel.Click += (s, e) =>
            {
                FieldHelper<TField> fieldHelper = TypeHelper.FieldHelper<TField>();

                if (fieldHelper.GetFieldType(field) == FieldType.Enum
                    && key is string stringValue)
                {
                    ITypeHelper? helper = fieldHelper.GetHelper(field);

                    if (helper != null)
                        key = helper.Parse(stringValue);
                }

                DataManager.ViewItems<TField, TDAO>(field, key);
            };
            captionLabel.MouseEnter += LabelMouseEnter;
            captionLabel.MouseLeave += LabelMouseLeave;

            IControlAccessor accessor = DataManager.Builder<TField, TDAO>(ControlScope.Inline)
                .Accessor(
                    $"{typeof(TField).Name}_{typeof(TDAO).Name}_AD{caption!}", 
                    FieldType.Label
                );
            accessor.Left = captionLabel.Right + HorizontalSpace;
            accessor.Top = location.Y;
            accessor.Parent = parent;
            accessor.Value = value;
            accessor.Control.Font = Styles.Font(FontStyle.Bold);
            OxControlHelper.AlignByBaseLine(accessor.Control, captionLabel);
            Add(new KeyValuePair<TField, object>(field, key), accessor);
            return accessor;
        }

        private void LabelMouseLeave(object? sender, EventArgs e)
        {
            OxLabel? label = (OxLabel?)sender;

            if (label == null)
                return;

            label.Font = new Font(label.Font, label.Font.Style & ~FontStyle.Underline);
            label.ForeColor = new OxColorHelper(label.ForeColor).HBluer(-6).Lighter(4);
        }

        private void LabelMouseEnter(object? sender, EventArgs e)
        {
            OxLabel? label = (OxLabel?)sender;

            if (label == null)
                return;

            label.Font = new Font(label.Font, label.Font.Style | FontStyle.Underline);
            label.ForeColor = new OxColorHelper(label.ForeColor).HDarker(4).Bluer(6);
        }

        private void ClearAccessorsParent()
        {
            foreach (IControlAccessor accessor in Values)
                accessor.Parent = null;
        }
    };
}