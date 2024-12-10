using OxDAOEngine.ControlFactory.Accessors;
using OxDAOEngine.ControlFactory.Initializers;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Fields;
using OxDAOEngine.Data.Links;
using OxLibrary;
using OxLibrary.Geometry;

namespace OxDAOEngine.ControlFactory.Controls
{
    public partial class LinkEditor<TField, TDAO> : CustomItemEditor<Link<TField>, TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        private ComboBoxAccessor<TField, TDAO> NameControl = default!;
        private IControlAccessor UrlControl = default!;
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
                .Accessor("LinkEditor:LinkName", FieldType.Extract, true);
            NameControl.Parent = this;
            NameControl.Left = 60;
            NameControl.Top = 8;
            NameControl.Width = OxSH.Sub(FormPanel.Width, NameControl.Left, 8);
            NameControl.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            NameControl.Height = 24;
            CreateLabel("Name", NameControl);
        }

        private void CreateUrlControl()
        {
            UrlControl = Context.Accessor("Link:Url", FieldType.Memo, true);
            UrlControl.Parent = this;
            UrlControl.Left = 60;
            UrlControl.Top = OxSH.Add(NameControl.Bottom, 8);
            UrlControl.Width = OxSH.Sub(FormPanel.Width, UrlControl.Left, 8);
            UrlControl.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            UrlControl.Height = 90;
            CreateLabel("URL", UrlControl);
        }

        protected override void CreateControls()
        {
            CreateNameControl();
            CreateUrlControl();
        }

        protected override short ContentHeight =>
            OxSH.Add(UrlControl.Bottom, 8);

        protected override void FillControls(Link<TField> item)
        {
            if (!item.Name.Equals(string.Empty) 
                && NameControl.ComboBox.Items.IndexOf(item.Name) < 0)
                NameControl.ComboBox.Items.Insert(0, item.Name);

            NameControl.Value = item.Name;
            UrlControl.Value = item.Url;
        }

        protected override void GrabControls(Link<TField> item)
        {
            item.Name = NameControl.StringValue;
            item.Url = UrlControl.StringValue;
        }

        protected override string EmptyMandatoryField() =>
            NameControl.IsEmpty
                ? "Link name"
                : UrlControl.IsEmpty
                    ? "Url"
                    : base.EmptyMandatoryField();
    }
}