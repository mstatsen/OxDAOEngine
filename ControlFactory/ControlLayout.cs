﻿using OxLibrary;
using OxLibrary.Controls;
using OxLibrary.Panels;
using OxDAOEngine.ControlFactory.Controls;
using OxDAOEngine.Data.Types;

namespace OxDAOEngine.ControlFactory
{
    public class ControlLayout<TField>
        where TField : notnull, Enum
    {
        public readonly OxWidth Space = OxWh.W8;
        public TField Field { get; set; } = default!;
        public Control? Parent { get; set; }
        public OxWidth Left { get; set; }
        public OxWidth Top { get; set; }
        public OxWidth Width { get; set; }
        public OxWidth Height { get; set; }
        public bool Visible { get; set; }
        public Color BackColor { get; set; }
        public Color FontColor { get; set; }
        public AnchorStyles Anchors { get; set; }
        public OxDock Dock { get; set; }
        public string FontFamily { get; set; } = string.Empty;
        public float FontSize { get; set; }
        public FontStyle FontStyle { get; set; }
        public ControlCaptionVariant CaptionVariant { get; set; }
        public bool WrapLabel { get; set; }
        public Color LabelColor { get; set; }
        public FontStyle LabelStyle { get; set; }
        public OxWidth MaximumLabelWidth { get; set; }
        public bool AutoSize { get; set; }
        public bool SupportClickedLabels { get; set; } = false;

        public OxWidth Right => Left | Width;
        public OxWidth Bottom => Top | Height;

        public void Clear()
        {
            Field = default!;
            Parent = null;
            Left = 0;
            Top = 0;
            Width = OxWh.W100;
            Height = OxWh.W28;
            Visible = true;
            BackColor = Color.FromKnownColor(KnownColor.Window);
            FontColor = Color.FromKnownColor(KnownColor.WindowText);
            CaptionVariant = ControlCaptionVariant.Left;
            WrapLabel = false;
            Anchors = AnchorStyles.Top | AnchorStyles.Left;
            Dock = OxDock.None;
            FontFamily = Styles.FontFamily;
            FontSize = Styles.DefaultFontSize;
            FontStyle = FontStyle.Regular;
            LabelColor = Color.FromKnownColor(KnownColor.WindowText);
            LabelStyle = FontStyle.Regular;
            MaximumLabelWidth = OxWh.W64;
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

            OxLabel? label = control.Tag as OxLabel;

            if (label is not null)
                label.Tag = control;

            return new PlacedControl<TField> (control, label, this);
        }

        private void ApplyLayoutToControl(Control control)
        {
            if (control is null)
                return;

            control.Parent = Parent;
            control.Left = OxWh.Int(Left);
            control.Top = OxWh.Int(Top);

            if (control is OxPanel pane)
                pane.Size = new(Width, Height);
            else
            {
                control.Width = OxWh.Int(Width);
                control.Height = OxWh.Int(Height);
            }

            control.Visible = Visible;
            control.ForeColor = FontColor;
            control.Dock = OxDockHelper.Dock(Dock);
            control.Font = new(FontFamily, FontSize, FontStyle);
            control.AutoSize = AutoSize;
            control.Anchor = Anchors;
            SetControlBackColor(control);
        }

        private void SetControlBackColor(Control control)
        {
            if (BackColor.Equals(Color.Transparent) 
                && Parent is not null)
                control.BackColor = Parent.BackColor;
            else
                BackColor = Color.FromArgb(255, BackColor);

            switch (control)
            {
                case IColoredCustomControl customControl:
                    customControl.ControlColor = BackColor;
                    break;
                case OxPanel pane:
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
            if (label is null)
                label = CreateLabel();
            else
            if (label.IsDisposed)
                return null;

            label.Text = TypeHelper.Caption(Field);
            label.Parent = (IOxControlContainer?)Parent;
            label.ForeColor = LabelColor;

            if (LabelsForeColors.ContainsKey(label))
                LabelsForeColors.Remove(label);

            LabelsForeColors.Add(label, LabelColor);

            label.BackColor = Color.Transparent;
            label.Visible = Visible;
            label.Font = new(FontFamily, FontSize - 1, LabelStyle);

            if (SupportClickedLabels)
            {
                label.Cursor = Cursors.Hand;
                label.MouseEnter -= LabelMouseEnter;
                label.MouseLeave -= LabelMouseLeave;
                label.MouseEnter += LabelMouseEnter;
                label.MouseLeave += LabelMouseLeave;
            }

            CalcLabel(label, control);
            return label;
        }

        private readonly Dictionary<OxLabel, Color> LabelsForeColors = new();

        private void LabelMouseLeave(object? sender, EventArgs e)
        {
            OxLabel? label = (OxLabel?)sender;

            if (label is null)
                return;

            label.Font = new(label.Font, label.Font.Style & ~FontStyle.Underline);
            label.ForeColor = LabelsForeColors[label];
        }

        private void LabelMouseEnter(object? sender, EventArgs e)
        {
            OxLabel? label = (OxLabel?)sender;

            if (label is null)
                return;

            label.Font = new(label.Font, label.Font.Style | FontStyle.Underline);
            label.ForeColor = new OxColorHelper(label.ForeColor).HLighter().Bluer();
        }

        private void CalcLabel(OxLabel? label, Control? control)
        {
            if (label is null)
                return;

            if (WrapLabel)
                label.MaximumSize = new OxSize(MaximumLabelWidth, OxWh.W0);

            label.AutoSize = true;
            label.Visible = 
                Visible 
                    && CaptionVariant is not ControlCaptionVariant.None; 

            switch (CaptionVariant)
            {
                case ControlCaptionVariant.Left:
                    label.Left = Left - label.Width - Space;

                    if (control is not null)
                        OxControlHelper.AlignByBaseLine(control, label);
                    break;
                case ControlCaptionVariant.Top:
                    label.Left = OxWh.Sub(Left, OxWh.W2);
                    label.Top = OxWh.Sub(Top, OxWh.W13);
                    break;
                case ControlCaptionVariant.None:
                    label.Text = string.Empty;
                    break;
            }
        }

        private OxLabel CreateLabel() => new();

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
            if (fixedLayout is not null)
                Top = fixedLayout.Bottom + offset;
        }

        public void OffsetVertical(ControlLayout<TField>? fixedLayout, bool withMargins = true) =>
            OffsetVertical(fixedLayout, withMargins ? VerticalControlMargin : 0);
    }
}