using OxLibrary;
using OxLibrary.Controls;
using OxLibrary.Panels;
using OxDAOEngine.ControlFactory;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Types;
using OxDAOEngine.Settings;
using OxDAOEngine.Settings.Data;
using OxDAOEngine.View.Types;

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
            Margin.Size = OxWh.W2;
            HandHoverCursor = true;
            fontColors = new OxColorHelper(DefaultForeColor);
            Size = new(IconWidth, IconHeight);
        }

        private IconSize IconsSize => ListController.Settings.IconsSize;

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
                case IconClickVariant.ShowKey:
                    ListController.ShowItemKey(item);
                    break;
                case IconClickVariant.ShowCard:
                    ListController.ViewItem(item, ItemViewMode.WithEditLinks);
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

            if (item is not null)
                builder.FillControls(item);

            LayoutControls();
            AfterLayoutControls();
            SetClickHandlers();
        }

        private void AfterLayoutControls()
        {
            if (ImageMapping is null
                || TitleMapping is null
                || !IconsSize.Equals(IconSize.Thumbnails))
                return;

            ToolTipText = builder[TitleMapping.Field].Text;
            ((OxPicture)builder[ImageMapping.Field].Control).ToolTipText = builder[TitleMapping.Field].Text;
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

            if (LeftMapping is not null)
                FillLeftLayout();

            if (RightMapping is not null)
                FillRightLayout();
        }

        private void FillTitleLayout()
        {
            if (TitleMapping is null) 
                return;

            ControlLayout<TField> titleLayout = layouter.AddFromTemplate(TitleMapping.Field);
            titleLayout.AutoSize = false;
            titleLayout.Top = OxWh.Int(OxWh.Sub(OxWh.Div(IconWidth, OxWh.W2), OxWh.W2));
            titleLayout.Width = OxWh.Int(IconWidth);
            titleLayout.Height = OxWh.Int(OxWh.Sub(IconHeight, titleLayout.Top));
            titleLayout.FontStyle = FontStyle.Bold;
            titleLayout.FontColor = fontColors.BaseColor;
            titleLayout.Visible = IconsSize is not IconSize.Thumbnails;
        }

        private void FillImageLayout()
        {
            if (ImageMapping is null)
                return;

            ControlLayout<TField> imageLayout = layouter.AddFromTemplate(ImageMapping.Field);
            imageLayout.Top = IconsSize.Equals(IconSize.Thumbnails) ? 0 : 1;
            imageLayout.Width = OxWh.Int(IconWidth);
            imageLayout.Height = OxWh.Int(OxWh.Sub(OxWh.Div(IconWidth, OxWh.W2), OxWh.W3));
        }

        private void FillLeftLayout()
        {
            if (LeftMapping is null)
                return;

            ControlLayout<TField> leftLayout = layouter.AddFromTemplate(LeftMapping.Field);
            leftLayout.Top = 5;
            leftLayout.FontSize -= sizeHelper.FontSizeDelta(IconsSize);
        }

        private void FillRightLayout()
        {
            if (RightMapping is null)
                return;

            ControlLayout<TField> rightLayout = layouter.AddFromTemplate(RightMapping.Field);
            rightLayout.Top = OxWh.Int(OxWh.Sub(OxWh.Div(IconWidth, OxWh.W2), OxWh.W20));
            rightLayout.FontSize -= sizeHelper.FontSizeDelta(ListController.Settings.IconsSize);
        }

        private void ClearLayoutTemplate()
        {
            layouter.Template.Parent = this;
            layouter.Template.Left = 0;
            layouter.Template.Top = 0;
            layouter.Template.Width = OxWh.Int(sizeHelper.AddInfoWidth(ListController.Settings.IconsSize));
            layouter.Template.CaptionVariant = ControlCaptionVariant.None;
            layouter.Template.WrapLabel = true;
            layouter.Template.MaximumLabelWidth = 80;
            layouter.Template.BackColor = BackColor;
            layouter.Template.FontColor = fontColors.BaseColor;
            layouter.Template.FontStyle = FontStyle.Bold | FontStyle.Italic;
            layouter.Template.FontSize = sizeHelper.FontSize(ListController.Settings.IconsSize);
            layouter.Template.LabelColor = fontColors.Lighter();
            layouter.Template.LabelStyle = FontStyle.Italic;
            layouter.Template.AutoSize = true;
        }

        private void LayoutControls()
        {
            layouter.LayoutControls();
            placedControls.Clear();

            if (Mapping is not null)
                foreach (IconMapping<TField> mapping in Mapping)
                {
                    Control? control = layouter.PlacedControl(mapping.Field)?.Control;

                    if (control is not null)
                        placedControls.Add(control);
                }

            if (LeftMapping is not null)
            {
                OxLabel? leftControl = (OxLabel?)layouter.PlacedControl(LeftMapping.Field)?.Control;

                if (leftControl is not null)
                {
                    leftControl.Left = OxWh.Int(OxWh.Sub(OxWh.W12, sizeHelper.LeftDelta(ListController.Settings.IconsSize)));
                    leftControl.TextAlign = ContentAlignment.MiddleLeft;
                    leftControl.BringToFront();
                }
            }

            if (RightMapping is not null)
            {
                OxLabel? rightControl = (OxLabel?)layouter.PlacedControl(RightMapping.Field)?.Control;

                if (rightControl is not null)
                {
                    rightControl.Left =
                        OxWh.Int(
                            OxWh.Add(
                                OxWh.Sub(IconWidth, rightControl.Width - 12),
                                sizeHelper.LeftDelta(ListController.Settings.IconsSize)
                            )
                        );
                    rightControl.TextAlign = ContentAlignment.MiddleLeft;
                    rightControl.BringToFront();
                }
            }

            if (TitleMapping is not null)
            {
                OxLabel? titleControl = (OxLabel?)layouter.PlacedControl(TitleMapping.Field)?.Control;

                if (titleControl is not null)
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
                control.BackColor = BackColor;
        }

        public void ApplySettings() { }

        private OxWidth IconWidth =>
            sizeHelper.Width(ListController.Settings.IconsSize);

        private OxWidth IconHeight =>
            sizeHelper.Height(ListController.Settings.IconsSize);

        public TDAO? Item
        {
            get => item;
            set
            {
                if (item is not null)
                    item.ChangeHandler -= ItemChangeHandler;

                Size = new(IconWidth, IconHeight);
                item = value;

                if (item is not null)
                    item.ChangeHandler += ItemChangeHandler;

                PrepareControls();
                PrepareColors();
            }
        }

        private ItemsView<TField, TDAO>? itemsView;

        public ItemsView<TField, TDAO>? ItemsView
        {
            get => itemsView;
            set => itemsView = value;
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