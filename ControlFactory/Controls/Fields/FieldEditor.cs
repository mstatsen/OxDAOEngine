﻿using OxXMLEngine.ControlFactory.Accessors;
using OxXMLEngine.Data;
using OxXMLEngine.Data.Fields;
using OxXMLEngine.Data.Types;

namespace OxXMLEngine.ControlFactory.Controls
{
    public partial class FieldEditor<TField, TDAO> 
        : ListItemEditor<FieldColumn<TField>, TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        private FieldAccessor<TField, TDAO> FieldControl = default!;

        public override void RenewData()
        {
            base.RenewData();

            if (ExistingItems != null)
            FieldControl.ExcludedList = new FieldColumns<TField>
            {
                ObjectList = ExistingItems
            }.Fields;
        }

        protected override string Title => "Field";

        private void CreateFieldControl()
        {
            FieldControl = (FieldAccessor<TField, TDAO>)Context.Builder
                .Accessor(TypeHelper.FieldHelper<TField>().FieldMetaData);
            FieldControl.Parent = this;
            FieldControl.Left = 12;
            FieldControl.Top = 12;
            FieldControl.Width = MainPanel.ContentContainer.Width - 24;
            FieldControl.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
        }

        protected override void CreateControls() =>
            CreateFieldControl();

        protected override int ContentWidth => 300;
        protected override int ContentHeight => FieldControl.Bottom + 8;

        protected override void FillControls(FieldColumn<TField> item) =>
            FieldControl.Value = item.Field;

        protected override void GrabControls(FieldColumn<TField> item) =>
            item.Field = FieldControl.EnumValue;

        protected override string EmptyMandatoryField() =>
            FieldControl.IsEmpty ? "Field" : base.EmptyMandatoryField();

        protected override void PrepareControlColors() => 
            FieldControl.Control.BackColor = MainPanel.Colors.Lighter(6);
    }
}
