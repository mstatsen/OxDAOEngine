using OxDAOEngine.ControlFactory.Accessors;
using OxDAOEngine.ControlFactory.Controls;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Fields;
using OxDAOEngine.Data.Filter;
using OxDAOEngine.Data.Filter.Types;
using OxLibrary;
using OxLibrary.Controls;

namespace OxDAOEngine.ControlFactory.Filter
{
    public partial class CategoryEditor<TField, TDAO> : ListItemEditor<Category<TField, TDAO>, TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        private IControlAccessor TypeControl = default!;
        private IControlAccessor NameControl = default!;
        private IControlAccessor FieldControl = default!;
        private IControlAccessor FiltrationControl = default!;
        private FilterPanel<TField, TDAO> FilterPanel = default!;

        public override Bitmap FormIcon => OxIcons.Link;

        private IControlAccessor CreateSimpleControl(string key, FieldType fieldType, string caption, int top, int width = -1)
        {
            IControlAccessor result = Builder.Accessor(key, fieldType);
            result.Parent = this;
            result.Left = 74;
            result.Top = top;

            if (width == -1)
            {
                result.Width = MainPanel.ContentContainer.Width - result.Left - 8;
                result.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            }
            else
                result.Width = width;


            result.Height = 24;
            result.Control.Tag = CreateLabel(caption, result);
            return result;
        }

        protected override void CreateControls()
        {
            TypeControl = CreateSimpleControl("Category:Type", FieldType.Enum, "Type", 8);
            TypeControl.ValueChangeHandler += TypeChangedHandler;
            NameControl = CreateSimpleControl("Category:Name", FieldType.String, "Name", TypeControl.Bottom + 4);
            FieldControl = CreateSimpleControl("Category:Field", FieldType.MetaData, "Field", NameControl.Top);
            FiltrationControl = CreateSimpleControl("Category:Filtration", FieldType.Enum, "Filtration", NameControl.Bottom + 4);
            FilterPanel = new(Builder)
            {
                Parent = this,
                Top = FiltrationControl.Bottom + 4,
                Dock = DockStyle.Bottom
            };
            FilterPanel.SizeChanged += FilterPanelSizeChangedHandler;
            RecalcSize();
        }

        private void FilterPanelSizeChangedHandler(object? sender, EventArgs e) =>
            RecalcSize();

        private CategoryType Type => TypeControl.EnumValue<CategoryType>();
        private bool IsFilterCategory => Type.Equals(CategoryType.Filter);

        private void TypeChangedHandler(object? sender, EventArgs e)
        {
            SetControlsVisible();
            RecalcSize();
        }

        private void SetControlsVisible()
        {
            if (IsFilterCategory)
            {
                NameControl.Visible = true;
                FiltrationControl.Visible = true;
                ((OxLabel)FiltrationControl.Control.Tag).Visible = true;
                FilterPanel.Visible = true;
                FieldControl.Visible = false;
            }
            else
            {
                NameControl.Visible = false;
                FiltrationControl.Visible = false;
                ((OxLabel)FiltrationControl.Control.Tag).Visible = false;
                FilterPanel.Visible = false;
                FieldControl.Visible = true;
            }

            RecalcSize();
        }

        protected override int ContentWidth => 600;

        protected override int ContentHeight =>
            IsFilterCategory
                ? FiltrationControl.Bottom + FilterPanel.Height
                : (FieldControl.Bottom + 8);

        protected override void FillControls(Category<TField, TDAO> item)
        {
            TypeControl.Value = item.Type;
            NameControl.Value = item.Name;
            FieldControl.Value = item.Field;
            FiltrationControl.Value = item.Filtration;
            FilterPanel.Filter = item.Filter;
            SetControlsVisible();
        }

        protected override void GrabControls(Category<TField, TDAO> item)
        {
            item.Type = TypeControl.EnumValue<CategoryType>();
            item.Name = NameControl.StringValue;
            item.Field = (TField)FieldControl.Value!;
            item.Filtration = FiltrationControl.EnumValue<FiltrationType>();
            item.Filter = FilterPanel.Filter;
        }

        protected override string EmptyMandatoryField() =>
            IsFilterCategory
                ? TypeControl.IsEmpty
                    ? "Category type"
                    : NameControl.IsEmpty
                        ? "Category name"
                        : base.EmptyMandatoryField()
                : FieldControl.IsEmpty 
                    ? "Field"
                    : base.EmptyMandatoryField();

        protected override void PrepareControlColors()
        {
            base.PrepareControlColors();
            FilterPanel.BaseColor = BaseColor;
        }
    }
}