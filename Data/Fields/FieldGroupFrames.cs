﻿using OxLibrary.Panels;
using OxDAOEngine.Data.Types;

namespace OxDAOEngine.Data.Fields
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
                group.Value.Size = new
                (
                    Width(group.Key),
                    Height(group.Key)
                );
        }

        private int Height(TFieldGroup group)
        {
            if (!fieldGroupHelper.IsCalcedHeightGroup(group))
                return fieldGroupHelper.DefaultGroupHeight(group);

            OxFrame groupFrame = this[group];
            int height = 0;

            foreach (Control control in groupFrame.Controls)
                if (control.Visible)
                    height = Math.Max(control.Bottom, height);

            return height + 8;
        }

        public int Width(TFieldGroup group) => fieldGroupHelper.GroupWidth(group);
        public DockStyle Dock(TFieldGroup group) => fieldGroupHelper.GroupDock(group);
    }
}