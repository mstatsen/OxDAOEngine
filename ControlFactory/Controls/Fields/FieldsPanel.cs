using OxLibrary;
using OxLibrary.Controls;
using OxLibrary.Panels;
using OxDAOEngine.ControlFactory.Accessors;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Fields;
using OxDAOEngine.Data.Types;
using OxDAOEngine.Data.Fields.Types;

namespace OxDAOEngine.ControlFactory.Controls.Fields
{
    public partial class FieldsPanel<TField, TDAO> : OxFunctionsPanel, IFieldsPanel
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public FieldColumns<TField> Fields
        {
            get => (FieldColumns<TField>)fieldsAccessor.Value!;
            set => fieldsAccessor.Value = value;
        }

        public FieldsPanel(FieldsVariant variant) : base(new(240, 86))
        {
            Variant = variant;
            SetTitle();
            BaseColor = TypeHelper.BaseColor(Variant);

            FieldVariantHelper variantHelper = TypeHelper.Helper<FieldVariantHelper>();

            fieldsAccessor = DataManager.Builder<TField, TDAO>(variantHelper.Scope(Variant)).FieldListAccessor(Variant);
            fieldsAccessor.Parent = this;
            fieldsAccessor.Dock = OxDock.Fill;

            FieldsControl<TField, TDAO> fieldsControl = (FieldsControl<TField, TDAO>)fieldsAccessor.Control;
            fieldsControl.BaseColor = BaseColor;
            fieldsControl.AllowSorting = FieldVariantHelper.Sortable(Variant);
            fieldsControl.GetMaximumCount += GetPossibleItemsCount;

            SetToolButtonsVisible();
        }

        public override Color DefaultColor => EngineStyles.FieldsColor;

        protected override void PrepareInnerComponents()
        {
            base.PrepareInnerComponents();
            CreateToolButtons();
        }

        private void ToolButtonClick(object? sender, EventArgs e)
        {
            if (sender is not null)
                ResetFields((FieldsFilling)((Control)sender).Tag);
        }

        public FieldHelper<TField> fieldHelper = TypeHelper.FieldHelper<TField>();
        public void ResetFields(FieldsFilling filling = FieldsFilling.Default) =>
            Fields = fieldHelper.Columns(Variant, filling);

        private void SetTitle() =>
            Text = TypeHelper.Name(Variant);

        private int GetPossibleItemsCount() =>
            fieldHelper.FullListCount(Variant);

        private void CreateToolButton(FieldsFilling filling)
        {
            FieldsFillingHelper helper = TypeHelper.Helper<FieldsFillingHelper>();
            OxButton button = new(helper.Name(filling), helper.ButtonIcon(filling))
            {
                Font = OxStyles.Font(-1, FontStyle.Bold),
                ToolTipText = helper.FullName(filling),
                Size = new(helper.ButtonWidth(filling), 23),
                Tag = filling
            };
            button.Click += ToolButtonClick;
            toolButtons.Add(filling, button);
        }

        private void CreateToolButtons()
        {
            foreach (FieldsFilling filling in TypeHelper.All<FieldsFilling>())
                CreateToolButton(filling);

            foreach (OxButton button in toolButtons.Values.Reverse())
                Header.AddButton(button);
        }

        private void SetToolButtonsVisible()
        {
            foreach (var item in toolButtons)
            {
                if (item.Key is FieldsFilling.Clear)
                    continue;

                item.Value.SetVisible(fieldHelper.Columns(Variant, item.Key)?.Count > 0);
            }
        }

        private readonly IControlAccessor fieldsAccessor;

        public FieldsVariant Variant { get; private set; }

        List<object> IFieldsPanel.Fields
        {
            get => Fields.ObjectList;
            set =>
                fieldsAccessor.Value =
                    new FieldColumns<TField>
                    {
                        ObjectList = value
                    };
        }

        private readonly Dictionary<FieldsFilling, OxButton> toolButtons = new();
    }
}
