using OxDAOEngine.ControlFactory.Accessors;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Fields;
using OxDAOEngine.Data.Filter;
using OxDAOEngine.Data.Filter.Types;
using OxLibrary;

namespace OxDAOEngine.ControlFactory.Controls.Filter
{
    public partial class CategoryEditor<TField, TDAO> : ListItemEditor<Category<TField, TDAO>, TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        private IControlAccessor NameControl = default!;
        private IControlAccessor TypeControl = default!;

        public override Bitmap FormIcon => OxIcons.Link;

        private IControlAccessor CreateSimpleControl(string key, FieldType fieldType, string caption, int top, int width = -1)
        {
            IControlAccessor result = Builder.Accessor(key, fieldType);
            result.Parent = this;
            result.Left = 60;
            result.Top = top;

            if (width == -1)
            {
                result.Width = MainPanel.ContentContainer.Width - result.Left - 8;
                result.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            }
            else
                result.Width = width;


            result.Height = 24;
            CreateLabel(caption, result);
            return result;
        }

        protected override void CreateControls()
        {
            NameControl = CreateSimpleControl("Category:Name", FieldType.String, "Name", 8);
            TypeControl = CreateSimpleControl("Category:Type", FieldType.Enum, "Type", NameControl.Bottom + 4);
        }

        protected override int ContentHeight => 
            TypeControl.Bottom + 8;

        protected override void FillControls(Category<TField, TDAO> item)
        {
            NameControl.Value = item.Name;
            TypeControl.Value = item.Type;
        }

        protected override void GrabControls(Category<TField, TDAO> item)
        {
            item.Name = NameControl.StringValue;
            item.Type = TypeControl.EnumValue<CategoryType>();
        }

        protected override string EmptyMandatoryField() =>
            NameControl.IsEmpty
                ? "Category name"
                : TypeControl.IsEmpty
                    ? "Category type" 
                    : base.EmptyMandatoryField();
    }
}