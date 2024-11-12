using OxLibrary.Panels;
using OxDAOEngine.ControlFactory.Accessors;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Fields;
using OxDAOEngine.Data.Types;
using OxDAOEngine.Data.Fields.Types;
using OxDAOEngine.Data.Filter;

namespace OxDAOEngine.ControlFactory.Filter
{
    public partial class CategroiesPanel<TField, TDAO> : OxFunctionsPanel, ICategoriesPanel
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public Categories<TField, TDAO> Categories
        {
            get => (Categories<TField, TDAO>)categoriesAccessor.Value!;
            set => categoriesAccessor.Value = value;
        }

        public CategroiesPanel() : base(new(240, 86))
        {
            Text = "Categories";
            categoriesAccessor = DataManager.Builder<TField, TDAO>(ControlScope.Category).CategoriesAccessor();
            categoriesAccessor.Parent = ContentContainer;
            categoriesAccessor.Dock = DockStyle.Fill;

            CategoriesControl<TField, TDAO> categoriesControl = (CategoriesControl<TField, TDAO>)categoriesAccessor.Control;
            categoriesControl.BaseColor = BaseColor;
        }

        public override Color DefaultColor => EngineStyles.CategoryColor;

        public FieldHelper<TField> fieldHelper = TypeHelper.FieldHelper<TField>();

        private readonly IControlAccessor categoriesAccessor;

        public FieldsVariant Variant { get; private set; }

        IListDAO ICategoriesPanel.Categories
        {
            get => Categories;
            set => categoriesAccessor.Value = value;
        }
    }
}