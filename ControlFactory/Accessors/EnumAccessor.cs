﻿using OxLibrary.Controls;
using OxDAOEngine.ControlFactory.Context;
using OxDAOEngine.ControlFactory.Initializers;
using OxDAOEngine.ControlFactory.ValueAccessors;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Types;

namespace OxDAOEngine.ControlFactory.Accessors
{
    public class EnumAccessor<TField, TDAO, TItem> : ComboBoxAccessor<TField, TDAO>
        where TDAO : RootDAO<TField>, new()
        where TField : notnull, Enum
        where TItem : Enum
    {
        public EnumAccessor(IBuilderContext<TField, TDAO> context) : base(context) { }

        protected override Control CreateControl() =>
            Context.MultipleValue
                ? new OxCheckComboBox<EnumItemObject<TItem>>()
                : base.CreateControl();

        protected virtual bool AvailableValue(TItem value) =>
            base.AvailableValue(value) &&
            (Context.Initializer is not ITypedComboBoxInitializer<TItem> initializer || 
                initializer.AvailableValue(value));

        private void ClearItems()
        {
            if (Context.MultipleValue)
                CheckComboBox.Items.Clear();
            else
                ComboBox.Items.Clear();
        }

        private OxCheckComboBox<EnumItemObject<TItem>> CheckComboBox =>
            (OxCheckComboBox<EnumItemObject<TItem>>)Control;

        protected override void InitControl()
        {
            ClearItems();
            TypeObjectList<TItem> items = new();

            foreach (TItem value in TypeHelper.All<TItem>())
                if (AvailableValue(value))
                {
                    EnumItemObject<TItem>? itemObject = TypeHelper.TypeObject<TItem>(value);

                    if (itemObject != null)
                        items.Add(itemObject);
                }

            items.Sort();
            AddRange(items.ToArray());

            if ((ComboBox.SelectedIndex == -1) && ComboBox.Items.Count > 0)
                ComboBox.SelectedIndex = 0;

            base.InitControl();
        }

        private void AddRange(EnumItemObject<TItem>[] items)
        {
            EnumItemObject<TItem>? emptyItem = TypeHelper.TypeObject<TItem>(TypeHelper.EmptyValue<TItem>());

            if (Context.MultipleValue)
            {
                CheckComboBox.Items.AddRange(items);

                if (emptyItem != null)
                    CheckComboBox.SelectedItem = emptyItem;
            }
            else
            {
                ComboBox.Items.AddRange(items);

                if (emptyItem != null)
                    ComboBox.SelectedItem = emptyItem;
            }
        }

        public override bool IsEmpty => 
            base.IsEmpty || Value is NullObject;

        public TItem EnumValue =>
            Context.MultipleValue ? default! : EnumValue<TItem>();

        protected override ValueAccessor CreateValueAccessor() =>
            Context.MultipleValue
                ? new TypedCheckComboBoxValueAccessor<TItem>()
                : new TypedComboBoxValueAccessor<TItem>();

        protected override void AssignValueChangeHanlderToControl(EventHandler? value) => 
            ComboBox.SelectionChangeCommitted += value;

        protected override void UnAssignValueChangeHanlderToControl(EventHandler? value) => 
            ComboBox.SelectionChangeCommitted -= value;
    }
}
