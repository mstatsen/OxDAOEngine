using OxDAOEngine.ControlFactory.Accessors;
using OxDAOEngine.ControlFactory.Initializers;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Filter;
using OxDAOEngine.Data.Sorting;
using OxDAOEngine.Data.Types;

namespace OxDAOEngine.ControlFactory.Controls
{
    public partial class SortingEditor<TField, TDAO> : ListItemEditor<FieldSorting<TField, TDAO>, TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        private FieldAccessor<TField, TDAO> FieldControl = default!;
        private IControlAccessor DirectionControl = default!;

        public override void RenewData()
        {
            base.RenewData();
            if (ExistingItems != null)
                FieldControl!.ExcludedList = new FieldSortings<TField, TDAO>
                {
                    ObjectList = ExistingItems
                }.Fields;
        }

        private void CreateFieldControl()
        {
            FieldControl = (FieldAccessor<TField, TDAO>)Context.Builder.Accessor(TypeHelper.FieldHelper<TField>().FieldMetaData);
            FieldControl.Parent = this;
            FieldControl.Left = 80;
            FieldControl.Top = 8;
            FieldControl.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            FieldControl.Height = 24;
            FieldControl.Width = MainPanel.ContentContainer.Width - FieldControl.Left - 8;
            FieldControl.Control.BackColor = MainPanel.Colors.Lighter(7);
            FieldControl.Context.Initializer = new FieldsInitializer<TField>(ControlScope.Sorting);
        }

        private void CreateDirectionControl()
        {
            DirectionControl = Context.Builder.EnumAccessor<SortOrder>();
            DirectionControl.Parent = this;
            DirectionControl.Left = 80;
            DirectionControl.Top = FieldControl!.Bottom + 8;
            FieldControl.Width = MainPanel.ContentContainer.Width - FieldControl.Left - 8;
            FieldControl.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            FieldControl.Height *= 3;
        }

        protected override void CreateControls()
        {
            CreateFieldControl();
            CreateDirectionControl();
            CreateLabel("Field", FieldControl!);
            CreateLabel("Direction", DirectionControl!);
        }

        protected override int ContentHeight => DirectionControl!.Bottom + 8;
        protected override int ContentWidth => 300;

        protected override void FillControls(FieldSorting<TField, TDAO> item)
        {
            FieldControl!.Value = item.Field;
            DirectionControl!.Value = item.SortOrder;
        }

        protected override void PrepareControlColors()
        {
            FieldControl!.Control.BackColor = MainPanel.Colors.Lighter(6);
            DirectionControl!.Control.BackColor = MainPanel.Colors.Lighter(6);
        }

        protected override void GrabControls(FieldSorting<TField, TDAO> item)
        {
            item.Field = FieldControl!.EnumValue<TField>();
            item.SortOrder = DirectionControl!.EnumValue<SortOrder>();
        }

        protected override string EmptyMandatoryField() =>
            FieldControl!.IsEmpty
                ? "Field"
                : DirectionControl!.IsEmpty
                    ? "Direction"
                    : base.EmptyMandatoryField();
    }
}