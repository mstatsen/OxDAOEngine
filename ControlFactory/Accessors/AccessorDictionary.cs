using OxLibrary;
using OxLibrary.Controls;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Fields;

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

        private const int HorizontalSpace = 8;

        public IControlAccessor CreateAccessor(object key, Control parent, 
            string? caption, object value, Point location)
        {
            OxLabel captionLabel = new()
            {
                Parent = parent,
                Left = location.X,
                Text = caption,
                Font = Styles.Font(FontStyle.Italic),
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = true
            };

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
            Add(key, accessor);
            return accessor;
        }

        private void ClearAccessorsParent()
        {
            foreach (IControlAccessor accessor in Values)
                accessor.Parent = null;
        }
    };
}