using OxLibrary;
using OxLibrary.Controls;
using OxLibrary.Panels;
using OxXMLEngine.ControlFactory.Controls;
using OxXMLEngine.Data.Types;

namespace OxXMLEngine.ControlFactory
{
    public class ControlLayout<TField>
        where TField : notnull, Enum
    {
        public const int Space = 8;

        public TField Field = default!;
        public Control? Parent;
        public int Left;
        public int Top;
        public int Width;
        public int Height;
        public bool Visible;
        public Color BackColor;
        public Color FontColor;
        public AnchorStyles Anchors;
        public DockStyle Dock;
        public string FontFamily = string.Empty;
        public float FontSize;
        public FontStyle FontStyle;
        public ControlCaptionVariant CaptionVariant;
        public bool WrapLabel;
        public Color LabelColor;
        public FontStyle LabelStyle;
        public int MaximumLabelWidth;
        public bool AutoSize;

        public int Right => Left + Width;
        public int Bottom => Top + Height;

        public void Clear()
        {
            Field = default!;
            Parent = null;
            Left = 0;
            Top = 0;
            Width = 100;
            Height = 28;
            Visible = true;
            BackColor = Color.FromKnownColor(KnownColor.Window);
            FontColor = Color.FromKnownColor(KnownColor.WindowText);
            CaptionVariant = ControlCaptionVariant.Left;
            WrapLabel = false;
            Anchors = AnchorStyles.Top | AnchorStyles.Left;
            Dock = DockStyle.None;
            FontFamily = Styles.FontFamily;
            FontSize = Styles.DefaultFontSize;
            FontStyle = FontStyle.Regular;
            LabelColor = Color.FromKnownColor(KnownColor.WindowText);
            LabelStyle = FontStyle.Regular;
            MaximumLabelWidth = 64;
            AutoSize = false;
        }

        public void ApplyLayout(PlacedControl<TField> placedControl)
        {
            ApplyLayoutToControl(placedControl.Control);
            ApplyLayoutToLabel(placedControl.Label, placedControl.Control);
        }

        public void RecalcLabel(PlacedControl<TField> control) =>
            CalcLabel(control?.Label, control?.Control);

        public PlacedControl<TField> ApplyLayout(Control control)
        {
            ApplyLayoutToControl(control);
            control.Tag = ApplyLayoutToLabel(null, control);

            OxLabel? label = (OxLabel?)control.Tag;

            if (label != null)
                label.Tag = control;

            return new PlacedControl<TField> (control, label, this);
        }

        private void ApplyLayoutToControl(Control control)
        {
            if (control == null)
                return;

            control.Parent = Parent;
            control.Left = Left;
            control.Top = Top;

            if (control is OxPane pane)
                pane.SetContentSize(Width, Height);
            else
            {
                control.Width = Width;
                control.Height = Height;
            }

            control.Visible = Visible;
            control.ForeColor = FontColor;
            control.Dock = Dock;
            control.Font = new Font(FontFamily, FontSize, FontStyle);
            control.AutoSize = AutoSize;
            control.Anchor = Anchors;
            SetControlBackColor(control);
        }

        private void SetControlBackColor(Control control)
        {
            if (BackColor == Color.Transparent && Parent != null)
                control.BackColor = Parent.BackColor;
            else
                BackColor = Color.FromArgb(255, BackColor);

            switch (control)
            {
                case IColoredCustomControl customControl:
                    customControl.ControlColor = BackColor;
                    break;
                case OxPane pane:
                    pane.BaseColor = BackColor;
                    break;
                case OxCheckBox checkBox:
                    checkBox.BackColor = Color.Transparent;
                    break;
                default:
                    control.BackColor = BackColor;
                    break;
            }
        }

        private OxLabel? ApplyLayoutToLabel(OxLabel? label, Control control)
        {
            if (label == null)
                label = ControlLayout<TField>.CreateLabel();
            else
            if (label.IsDisposed)
                return null;

            label.Text = TypeHelper.Name(Field);
            label.Parent = Parent;
            label.Font = new Font(FontFamily, FontSize - 1, LabelStyle);
            label.ForeColor = LabelColor;
            label.BackColor = Color.Transparent;
            label.Visible = Visible;

            CalcLabel(label, control);
            return label;
        }

        private void CalcLabel(OxLabel? label, Control? control)
        {
            if (label == null)
                return;

            if (WrapLabel)
                label.MaximumSize = new Size(MaximumLabelWidth, 0);

            label.AutoSize = true;

            switch (CaptionVariant)
            {
                case ControlCaptionVariant.Left:
                    label.Left = Left - label.Width - Space;

                    if (control != null)
                        OxControlHelper.AlignByBaseLine(control, label);
                    break;
                case ControlCaptionVariant.Top:
                    label.Left = Left - 2;
                    label.Top = Top - 13;
                    break;
                case ControlCaptionVariant.None:
                    label.Text = string.Empty;
                    label.Visible = false;
                    break;
            }
        }

        private static OxLabel CreateLabel() => new();

        public ControlLayout() => Clear();

        public ControlLayout(TField field) : base() =>
            Field = field;

        public void CopyFrom(ControlLayout<TField> layout)
        {
            Parent = layout.Parent;
            Left = layout.Left;
            Top = layout.Top;
            Width = layout.Width;
            Height = layout.Height;
            Visible = layout.Visible;
            BackColor = layout.BackColor;
            FontColor = layout.FontColor;
            FontFamily = layout.FontFamily;
            FontSize = layout.FontSize;
            FontStyle = layout.FontStyle;
            CaptionVariant = layout.CaptionVariant;
            WrapLabel = layout.WrapLabel;
            Anchors = layout.Anchors;
            Dock = layout.Dock;
            LabelColor = layout.LabelColor;
            LabelStyle = layout.LabelStyle;
            MaximumLabelWidth = layout.MaximumLabelWidth;
            AutoSize = layout.AutoSize;
        }

        public const int VerticalControlMargin = 8;

        public void OffsetVertical(ControlLayout<TField>? fixedLayout, int offset)
        {
            if (fixedLayout != null)
                Top = fixedLayout.Bottom + offset;
        }

        public void OffsetVertical(ControlLayout<TField>? fixedLayout, bool withMargins = true) =>
            OffsetVertical(fixedLayout, withMargins ? VerticalControlMargin : 0);
    }
}