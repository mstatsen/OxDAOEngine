using OxLibrary;
using OxLibrary.Controls;
using OxLibrary.Geometry;
using OxLibrary.Handlers;
using OxDAOEngine.ControlFactory.Accessors;
using OxDAOEngine.ControlFactory.Controls;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Fields;
using OxDAOEngine.Data.Filter;
using OxDAOEngine.Data.Filter.Types;
using OxDAOEngine.Data.Types;

namespace OxDAOEngine.ControlFactory.Filter;

public partial class CategoryEditor<TField, TDAO> : CustomItemEditor<Category<TField, TDAO>, TField, TDAO>
    where TField : notnull, Enum
    where TDAO : RootDAO<TField>, new()
{
    private IControlAccessor TypeControl = default!;
    private IControlAccessor NameControl = default!;
    private IControlAccessor FieldControl = default!;
    private IControlAccessor BaseOnChildsControl = default!;
    private FilterPanel<TField, TDAO> FilterPanel = default!;

    public override Bitmap FormIcon => OxIcons.Link;

    private IControlAccessor CreateSimpleControl(string key, FieldType fieldType, string caption, short top, short width = -1)
    {
        IControlAccessor result = Builder.Accessor(key, fieldType);
        result.Parent = this;
        result.Left = 74;
        result.Top = top;

        if (fieldType is not FieldType.Boolean)
        {
            if (width is -1)
            {
                result.Width = OxSh.Sub(FormPanel.Width, result.Left, 8);
                result.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            }
            else
                result.Width = width;
        }

        result.Height = 24;

        if (!caption.Equals(string.Empty))
            result.Control.Tag = CreateLabel(caption, result);

        return result;
    }

    protected override void CreateControls()
    {
        TypeControl = CreateSimpleControl("Category:Type", FieldType.Enum, "Type", 8);
        TypeControl.ValueChangeHandler += TypeChangedHandler;
        NameControl = CreateSimpleControl("Category:Name", FieldType.String, "Name", OxSh.Add(TypeControl.Bottom, 4));
        FieldControl = CreateSimpleControl("Category:Field", FieldType.MetaData, "Field", NameControl.Top);
        BaseOnChildsControl = CreateSimpleControl("Category:BaseOnChilds", FieldType.Boolean, string.Empty, OxSh.Add(NameControl.Bottom, 4));
        ((OxCheckBox)BaseOnChildsControl.Control).AutoSize = OxB.T;
        BaseOnChildsControl.Left = 8;
        BaseOnChildsControl.Text = "Base on childs";
        BaseOnChildsControl.ValueChangeHandler += BaseOnChildsControlValueChangeHandler;

        FilterPanel = new(Builder)
        {
            Parent = FormPanel,
            Dock = OxDock.Bottom
        };
        FilterPanel.GetCategoryName += GetCategoryNameHandler;
        FilterPanel.SizeChanged += FilterPanelSizeChangedHandler;
        RecalcSize();
    }

    private string GetCategoryNameHandler() => 
        NameControl.StringValue;

    private void BaseOnChildsControlValueChangeHandler(object? sender, EventArgs e) =>
        SetControlsVisible();

    private void FilterPanelSizeChangedHandler(object sender, OxSizeChangedEventArgs args) =>
        RecalcSize();

    private CategoryType Type => TypeControl.EnumValue<CategoryType>();
    private bool IsFilterCategory => Type is CategoryType.Filter;

    private void TypeChangedHandler(object? sender, EventArgs e)
    {
        SetControlsVisible();
        RecalcSize();
    }

    private void SetControlsVisible()
    {
        if (IsFilterCategory)
        {
            BaseOnChildsControl.Visible = OxB.T;
            NameControl.Visible = OxB.T;
            ((OxLabel)NameControl.Control.Tag!).Visible = OxB.T;
            FilterPanel.SetVisible(!BaseOnChildsControl.BoolValue);
            FieldControl.Visible = OxB.F;
            ((OxLabel)FieldControl.Control.Tag!).Visible = OxB.F;
        }
        else
        {
            BaseOnChildsControl.Visible = OxB.F;
            NameControl.Visible = OxB.F;
            ((OxLabel)NameControl.Control.Tag!).Visible = OxB.F;
            FilterPanel.Visible = OxB.F;
            FieldControl.Visible = OxB.T;
            ((OxLabel)FieldControl.Control.Tag!).Visible = OxB.T;
        }
        
        RecalcSize();
    }

    protected override short ContentWidth => 600;

    protected override short ContentHeight =>
        OxSh.Short(
            IsFilterCategory
                ? BaseOnChildsControl.BoolValue
                    ? BaseOnChildsControl.Bottom + 8
                    : BaseOnChildsControl.Bottom + FilterPanel.Height + 4
                : FieldControl.Bottom + 8
        );

    protected override void FillControls(Category<TField, TDAO> item)
    {
        TypeControl.Value = item.Type;
        NameControl.Value = item.Name;
        FieldControl.Value = item.Field;
        BaseOnChildsControl.Value = item.BaseOnChilds;
        FilterPanel.Filter = item.Filter;
        SetControlsVisible();
    }

    protected override void GrabControls(Category<TField, TDAO> item)
    {
        item.Type = Type;
        item.Name = IsFilterCategory ? NameControl.StringValue : $"By {TypeHelper.Name(FieldControl.Value)}";
        item.Field = (TField)FieldControl.Value!;
        item.BaseOnChilds = BaseOnChildsControl.BoolValue;
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