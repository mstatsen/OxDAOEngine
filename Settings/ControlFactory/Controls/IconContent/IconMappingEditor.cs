using OxDAOEngine.ControlFactory;
using OxDAOEngine.ControlFactory.Accessors;
using OxDAOEngine.ControlFactory.Controls;
using OxDAOEngine.ControlFactory.Initializers;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Fields;
using OxDAOEngine.Data.Types;
using OxDAOEngine.Settings.ControlFactory.Initializers;
using OxDAOEngine.Settings.Data;
using OxDAOEngine.SystemEngine;
using OxDAOEngine.View.Types;
using OxLibrary;

namespace OxDAOEngine.Settings.ControlFactory.Controls
{
    public partial class IconMappingEditor<TField> : CustomItemEditor<IconMapping<TField>, DAOSetting, SystemRootDAO<DAOSetting>>
        where TField : notnull, Enum
    {
        private IControlAccessor ContentPartControl = default!;
        private IControlAccessor FieldControl = default!;

        public override void RenewData()
        {
            base.RenewData();

            if (ExistingItems is not null)
                iconContentPartInitializer.ExistingMappings = new ListDAO<IconMapping<TField>>(ExistingItems);

            ContentPartControl.RenewControl(true);
            FieldControl.RenewControl(true);
        }

        public IconMappingEditor() => 
            InitializeComponent();

        private readonly IconContentPartInitializer<TField> iconContentPartInitializer = new();

        private void CreateContentPartControl()
        {
            ContentPartControl = Context.Builder.Accessor<IconContent>();
            ContentPartControl.Context.SetInitializer(iconContentPartInitializer);
            ContentPartControl.Parent = this;
            ContentPartControl.Left = 64;
            ContentPartControl.Top = 12;
            ContentPartControl.Width =
                OxWh.Int(
                    OxWh.Sub(
                        OxWh.Sub(FormPanel.Width, ContentPartControl.Left),
                        OxWh.W8
                    )
                );
            ContentPartControl.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            CreateLabel("Part: ", ContentPartControl);
        }

        private void CreateFieldControl()
        {
            FieldControl = Context.Accessor("IconMapping:Field", FieldType.Enum);
            ((FieldsInitializer<TField>)FieldControl.Context.Initializer!).SetControlScope(ControlScope.IconView);
            FieldControl.Parent = this;
            FieldControl.Left = ContentPartControl!.Left;
            FieldControl.Top = ContentPartControl.Bottom + 8;
            FieldControl.Width =
                OxWh.Int(
                    OxWh.Sub(
                        OxWh.Sub(FormPanel.Width, FieldControl.Left),
                        OxWh.W8
                    )
                );
            FieldControl.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            CreateLabel("Field: ", FieldControl);
        }

        protected override void CreateControls()
        {
            CreateContentPartControl();
            CreateFieldControl();
        }

        protected override void FillControls(IconMapping<TField> item)
        {
            ContentPartControl.Value = item.Part;
            FieldControl.Value = item.Field;
        }

        protected override void GrabControls(IconMapping<TField> item)
        {
            item.Part = TypeHelper.Value<IconContent>(ContentPartControl.Value);
            item.Field = FieldControl.EnumValue<TField>() ?? default!;
        }

        protected override OxWidth ContentWidth => OxWh.W320;
        protected override OxWidth ContentHeight => OxWh.Add(FieldControl.Bottom, OxWh.W8);

        protected override string EmptyMandatoryField() =>
            ContentPartControl.IsEmpty 
                ? "ContentPart" 
                : base.EmptyMandatoryField();
    }
}