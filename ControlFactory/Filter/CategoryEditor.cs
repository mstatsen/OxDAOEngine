﻿using OxDAOEngine.ControlFactory.Accessors;
using OxDAOEngine.ControlFactory.Initializers;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Fields;
using OxDAOEngine.Data.Filter;
using OxLibrary;

namespace OxDAOEngine.ControlFactory.Controls
{
    public partial class CategoryEditor<TField, TDAO> : ListItemEditor<Category<TField, TDAO>, TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        private ComboBoxAccessor<TField, TDAO> NameControl = default!;
        public override Bitmap FormIcon => OxIcons.Link;

        public override void RenewData()
        {
            base.RenewData();

            ((LinkNameInitializer<TField, TDAO>)NameControl.Context.Initializer!).ExistingItems = ExistingItems;
            NameControl.Context.InitControl(NameControl);
        }

        private void CreateNameControl()
        {
            NameControl = (ComboBoxAccessor<TField, TDAO>)Context.Builder
                .Accessor("CategoryName", FieldType.String);
            NameControl.Parent = this;
            NameControl.Left = 60;
            NameControl.Top = 8;
            NameControl.Width = MainPanel.ContentContainer.Width - NameControl.Left - 8;
            NameControl.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            NameControl.Height = 24;
            CreateLabel("Name", NameControl);
        }

        protected override void CreateControls()
        {
            CreateNameControl();
        }

        protected override int ContentHeight => 
            NameControl.Bottom + 8;

        protected override void FillControls(Category<TField, TDAO> item)
        {
            NameControl.Value = item.Name;
        }

        protected override void GrabControls(Category<TField, TDAO> item)
        {
            item.Name = NameControl.StringValue;
        }

        protected override string EmptyMandatoryField() =>
            NameControl.IsEmpty
                ? "Category name"
                : base.EmptyMandatoryField();
    }
}