using OxLibrary;
using OxLibrary.Controls;
using OxLibrary.Panels;
using OxDAOEngine.ControlFactory;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Types;
using OxDAOEngine.Settings;
using OxDAOEngine.Settings.Data;

namespace OxDAOEngine.View
{
    public sealed class ItemIcon<TField, TDAO> : OxClickFrame, IItemView<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    { 
        public ItemIcon() : base()
        {
            builder = ListController.ControlFactory.Builder(ControlScope.IconView, true);
            Click += ClickHandler;
            layouter = builder.Layouter;
            Margins.SetSize(OxSize.Medium);
            HandHoverCursor = true;
            fontColors = new OxColorHelper(DefaultForeColor);
            SetContentSize(IconWidth, IconHeight);
        }

        private readonly IListController<TField, TDAO> ListController = 
            DataManager.ListController<TField, TDAO>();

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            builder.DisposeControls();
        }

        protected override void PrepareColors()
        {
            base.PrepareColors();
            RecolorControls();
        }

        private void ItemChangeHandler(object sender, DAOEntityEventArgs e) =>
            PrepareControls();

        private void ClickHandler(object? sender, EventArgs e)
        {
            switch (ListController.Settings.IconClickVariant)
            {
                case IconClickVariant.ShowCard:
                    ListController.ViewItem(item, ItemViewMode.WithEditLink);
                    break;
                case IconClickVariant.ShowEditor:
                    ListController.EditItem(item);
                    break;
            }
        }

        private ItemColorer<TField, TDAO> ItemColorer => 
            ListController.ControlFactory.ItemColorer;

        private void PrepareControls()
        {
            BaseColor = ItemColorer.BaseColor(item);
            fontColors.BaseColor = ItemColorer.ForeColor(item);
            PrepareLayouts();

            if (item != null)
                builder.FillControls(item);

            LayoutControls();
            SetClickHandlers();
        }

        private void ClearLayouts() =>
            layouter?.Layouts.Clear();

        private static ListDAO<IconMapping<TField>>? Mapping => 
            SettingsManager.DAOSettings<TField, TDAO>().IconMapping;
        private static IconMapping<TField>? PartMapping(IconContent part) =>
            Mapping?.Find(p => p.Part.Equals(part));

        private static IconMapping<TField>? ImageMapping => 
            ItemIcon<TField, TDAO>.PartMapping(IconContent.Image);
        private static IconMapping<TField>? TitleMapping => 
            ItemIcon<TField, TDAO>.PartMapping(IconContent.Title);
        private static IconMapping<TField>? LeftMapping => 
            ItemIcon<TField, TDAO>.PartMapping(IconContent.Left);
        private static IconMapping<TField>? RightMapping => 
            ItemIcon<TField, TDAO>.PartMapping(IconContent.Right);

        private void PrepareLayouts()
        {
            placedControls.Clear();
            ClearLayouts();
            ClearLayoutTemplate();
            FillImageLayout();
            FillTitleLayout();

            if (LeftMapping != null)
                FillLeftLayout();

            if (RightMapping != null)
                FillRightLayout();
        }

        private void FillTitleLayout()
        {
            if (TitleMapping == null) 
                return;

            ControlLayout<TField> titleLayout = layouter.AddFromTemplate(TitleMapping.Field);
            titleLayout.AutoSize = false;
            titleLayout.Top = IconWidth / 2 - 2;
            titleLayout.Width = IconWidth;
            titleLayout.Height = IconHeight - titleLayout.Top;
            titleLayout.FontStyle = FontStyle.Bold;
            titleLayout.FontColor = fontColors.BaseColor;
        }

        private void FillImageLayout()
        {
            if (ImageMapping == null)
                return;

            ControlLayout<TField> imageLayout = layouter.AddFromTemplate(ImageMapping.Field);
            imageLayout.Top = 1;
            imageLayout.Width = IconWidth;
            imageLayout.Height = IconWidth / 2 - 3;
        }

        private void FillLeftLayout()
        {
            if (LeftMapping == null)
                return;

            ControlLayout<TField> leftLayout = layouter.AddFromTemplate(LeftMapping.Field);
            leftLayout.Top = 5;
            leftLayout.FontSize -= IconSizeHelper.FontSizeDelta(ListController.Settings.IconsSize);
        }

        private void FillRightLayout()
        {
            if (RightMapping == null)
                return;

            ControlLayout<TField> rightLayout = layouter.AddFromTemplate(RightMapping.Field);
            rightLayout.Top = IconWidth / 2 - 20;
            rightLayout.FontSize -= IconSizeHelper.FontSizeDelta(ListController.Settings.IconsSize);
        }

        private void ClearLayoutTemplate()
        {
            layouter.Template.Parent = this;
            layouter.Template.Left = 0;
            layouter.Template.Top = 0;
            layouter.Template.Width = IconSizeHelper.AddInfoWidth(ListController.Settings.IconsSize);
            layouter.Template.CaptionVariant = ControlCaptionVariant.None;
            layouter.Template.WrapLabel = true;
            layouter.Template.MaximumLabelWidth = 80;
            layouter.Template.BackColor = BackColor;
            layouter.Template.FontColor = fontColors.BaseColor;
            layouter.Template.FontStyle = FontStyle.Bold | FontStyle.Italic;
            layouter.Template.FontSize = IconSizeHelper.FontSize(ListController.Settings.IconsSize);
            layouter.Template.LabelColor = fontColors.Lighter(1);
            layouter.Template.LabelStyle = FontStyle.Italic;
            layouter.Template.AutoSize = true;
        }

        private void LayoutControls()
        {
            layouter.LayoutControls();
            placedControls.Clear();

            if (Mapping != null)
                foreach (IconMapping<TField> mapping in Mapping)
                {
                    Control? control = layouter.PlacedControl(mapping.Field)?.Control;

                    if (control != null)
                        placedControls.Add(control);
                }

            if (LeftMapping != null)
            {
                OxLabel? leftControl = (OxLabel?)layouter.PlacedControl(LeftMapping.Field)?.Control;

                if (leftControl != null)
                {
                    leftControl.Left = 12 - IconSizeHelper.LeftDelta(ListController.Settings.IconsSize);
                    leftControl.TextAlign = ContentAlignment.MiddleLeft;
                    leftControl.BringToFront();
                }
            }

            if (RightMapping != null)
            {
                OxLabel? rightControl = (OxLabel?)layouter.PlacedControl(RightMapping.Field)?.Control;

                if (rightControl != null)
                {
                    rightControl.Left = IconWidth - rightControl.Width - 12
                        + IconSizeHelper.LeftDelta(ListController.Settings.IconsSize);
                    rightControl.TextAlign = ContentAlignment.MiddleLeft;
                    rightControl.BringToFront();
                }
            }

            if (TitleMapping != null)
            {
                OxLabel? titleControl = ((OxLabel?)layouter.PlacedControl(TitleMapping.Field)?.Control);

                if (titleControl != null)
                    titleControl.TextAlign = ContentAlignment.MiddleCenter;
            }
        }

        private void SetClickHandlers()
        {
            foreach (Control control in placedControls)
            {
                SetClickHandler(control);
                SetHoverHandlers(control);
            }
        }

        private void RecolorControls()
        {
            foreach (Control control in placedControls)
                control.BackColor = ContentContainer.BackColor;
        }

        public void ApplySettings() { }

        private int IconWidth =>
            IconSizeHelper.Width(ListController.Settings.IconsSize);

        private int IconHeight =>
            IconSizeHelper.Height(ListController.Settings.IconsSize);

        public TDAO? Item
        {
            get => item;
            set
            {
                if (item != null)
                    item.ChangeHandler -= ItemChangeHandler;

                item = value;

                if (item != null)
                    item.ChangeHandler += ItemChangeHandler;

                PrepareControls();
                PrepareColors();
            }
        }

        public OxPane AsPane => this;

        private TDAO? item;
        private readonly OxColorHelper fontColors;
        private readonly ControlBuilder<TField, TDAO> builder;
        private readonly List<Control> placedControls = new();
        private readonly IconSizeHelper sizeHelper = TypeHelper.Helper<IconSizeHelper>();
        private readonly ControlLayouter<TField, TDAO> layouter;
    }
}