using OxLibrary.Panels;
using OxXMLEngine.Data.Types;

namespace OxXMLEngine.Data.Fields
{
    public class FieldGroupFrames<TField, TFieldGroup> : Dictionary<TFieldGroup, OxFrame>
        where TField : notnull, Enum
        where TFieldGroup : notnull, Enum
    {
        private readonly FieldGroupHelper<TField, TFieldGroup> fieldGroupHelper = 
            TypeHelper.FieldGroupHelper<TField, TFieldGroup>();

        public void Add(FieldGroupFrames<TField, TFieldGroup> other)
        {
            foreach (var item in other)
                Add(item.Key, item.Value);
        }

        public void SetGroupsSize()
        {
            foreach (var group in this)
                group.Value.SetContentSize(
                    Width(group.Key),
                    Height(group.Key)
                );
        }

        private int Height(TFieldGroup group)
        {
            if (!fieldGroupHelper.IsCalcedHeightGroup(group))
                return fieldGroupHelper.DefaultGroupHeight(group);

            OxPanel groupFrame = this[group];
            int height = 0;

            foreach (Control control in groupFrame.ContentContainer.Controls)
                if (control.Visible)
                    height = Math.Max(control.Bottom, height);

            return height + 8;
        }

        public int Width(TFieldGroup group) => fieldGroupHelper.GroupWidth(group);
        public DockStyle Dock(TFieldGroup group) => fieldGroupHelper.GroupDock(group);
    }
}