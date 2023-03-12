using OxXMLEngine.Data.Types;

namespace OxXMLEngine.Data.Fields
{
    public abstract class FieldGroupHelper<TField, TGroup>
        : AbstractTypeHelper<TGroup>
        where TField : notnull, Enum
        where TGroup : notnull, Enum
    {
        public abstract List<TGroup> EditedList();

        public abstract TGroup Group(TField field);

        public List<TField> Fields(TGroup editedGroup)
        {
            List<TField> result = new();

            foreach (TField field in TypeHelper.All<TField>())
                if (EditedGroup(field).Equals(editedGroup))
                    result.Add(field);

            return result;
        }

        public virtual TGroup EditedGroup(TField field) =>
            Group(field);

        public abstract int GroupWidth(TGroup group);

        public virtual DockStyle GroupDock(TGroup group) =>
            DockStyle.Top;

        public abstract bool IsCalcedHeightGroup(TGroup group);

        public abstract int DefaultGroupHeight(TGroup group);
    }
}