using OxLibrary;
using OxLibrary.Controls;
using OxLibrary.Dialogs;
using OxLibrary.Panels;
using OxXMLEngine.ControlFactory.Controls;
using OxXMLEngine.Data.Fields;
using OxXMLEngine.Data.Types;

namespace OxXMLEngine.ControlFactory
{
    public class ControlLayout<TField>
        where TField : notnull, Enum
    {
        public const int Space = 8;

        public TField Field { get; set; } = default!;
        public Control? Parent { get; set; }
        public int Left { get; set; }
        public int Top { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool Visible { get; set; }
        public Color BackColor { get; set; }
        public Color FontColor { get; set; }
        public AnchorStyles Anchors { get; set; }
        public DockStyle Dock { get; set; }
        public string FontFamily { get; set; } = string.Empty;
        public float FontSize { get; set; }
        public FontStyle FontStyle { get; set; }
        public ControlCaptionVariant CaptionVariant { get; set; }
        public bool WrapLabel { get; set; }
        public Color LabelColor { get; set; }
        public FontStyle LabelStyle { get; set; }
        public int MaximumLabelWidth { get; set; }
        public bool AutoSize { get; set; }

        public bool SupportClickedLabels { get; set; } = false;

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
                label = CreateLabel();
            else
            if (label.IsDisposed)
                return null;

            label.Text = TypeHelper.Name(Field);
            label.Parent = Parent;
            label.ForeColor = LabelColor;

            if (LabelsForeColors.ContainsKey(label))
                LabelsForeColors.Remove(label);

            LabelsForeColors.Add(label, LabelColor);

            label.BackColor = Color.Transparent;
            label.Visible = Visible;
            label.Font = new Font(FontFamily, FontSize - 1, LabelStyle);

            if (SupportClickedLabels)
            {
                FieldType fieldType = TypeHelper.FieldHelper<TField>().GetFieldType(Field);

                if (fieldType == FieldType.Extract || fieldType == FieldType.Enum)
                {
                    label.Cursor = Cursors.Hand;
                    label.MouseEnter -= LabelMouseEnter;
                    label.MouseEnter += LabelMouseEnter;
                    label.MouseLeave -= LabelMouseLeave;
                    label.MouseLeave += LabelMouseLeave;
                }
            }

            CalcLabel(label, control);
            return label;
        }

        private readonly Dictionary<OxLabel, Color> LabelsForeColors = new();

        private void LabelMouseLeave(object? sender, EventArgs e)
        {
            OxLabel? label = (OxLabel?)sender;

            if (label == null)
                return;

            label.Font = new Font(label.Font, label.Font.Style & ~FontStyle.Underline);
            label.ForeColor = LabelsForeColors[label];
        }

        private void LabelMouseEnter(object? sender, EventArgs e)
        {
            OxLabel? label = (OxLabel?)sender;

            if (label == null)
                return;

            label.Font = new Font(label.Font, label.Font.Style | FontStyle.Underline);
            label.ForeColor = new OxColorHelper(label.ForeColor).HLighter(4).Bluer(4);
        }

        private void CalcLabel(OxLabel? label, Control? control)
        {
            if (label == null)
                return;

            if (WrapLabel)
                label.MaximumSize = new Size(MaximumLabelWidth, 0);

            label.AutoSize = true;
            label.Visible = Visible && CaptionVariant != ControlCaptionVariant.None; 

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
            SupportClickedLabels = layout.SupportClickedLabels;
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