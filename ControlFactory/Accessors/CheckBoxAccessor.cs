using OxLibrary.Controls;
using OxDAOEngine.ControlFactory.Context;
using OxDAOEngine.ControlFactory.ValueAccessors;
using OxDAOEngine.Data;
using OxLibrary.Panels;
using OxLibrary;

namespace OxDAOEngine.ControlFactory.Accessors
{
    public class CheckBoxAccessor<TField, TDAO> : ControlAccessor<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public CheckBoxAccessor(IBuilderContext<TField, TDAO> context) : base(context) { }

        protected override Control CreateControl()
        {
            OxCheckBox checkBox = new()
            {
                CheckAlign = ContentAlignment.MiddleRight,
                Width = 14
            };
            checkBox.CheckedChanged += SetReadOnlyPictureHandler;
            return checkBox;
        }

        private void SetReadOnlyPictureHandler(object? sender, EventArgs e) => 
            OnControlValueChanged(CheckBox.Checked);

        public ContentAlignment CheckAlign 
        {
            get => CheckBox.CheckAlign;
            set => CheckBox.CheckAlign = value;
        }

        protected override ValueAccessor CreateValueAccessor() => 
            new CheckBoxValueAccessor();

        protected override void AssignValueChangeHanlderToControl(EventHandler? value) =>
            CheckBox.CheckedChanged += value;

        protected override void UnAssignValueChangeHanlderToControl(EventHandler? value) =>
            CheckBox.CheckedChanged -= value;

        public override void Clear() =>
            Value = false;

        public OxCheckBox CheckBox =>
            (OxCheckBox)Control;

        private OxLabel ReadOnlyLabel = default!;
        private readonly OxPicture ReadOnlyPicture = new();

        private readonly int ReadOnlyPictureSize = 16;

        protected override Control? CreateReadOnlyControl()
        {
            OxPane readOnlyControl = new();
            ReadOnlyLabel = new OxLabel
            {
                Parent = readOnlyControl
            };
            ReadOnlyPicture.Parent = readOnlyControl;
            ReadOnlyPicture.Height = ReadOnlyPictureSize;
            ReadOnlyPicture.Width = ReadOnlyPictureSize;
            ReadOnlyPicture.MinimumSize = new(ReadOnlyPictureSize, ReadOnlyPictureSize);
            ReadOnlyPicture.Top = 0;
            ReadOnlyPicture.Left = readOnlyControl.Width - ReadOnlyPictureSize;
            ReadOnlyPicture.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            ReadOnlyPicture.PictureSize = ReadOnlyPictureSize;
            ReadOnlyPicture.Image = OxIcons.Tick;
            return readOnlyControl;
        }

        protected override void OnControlSizeChanged()
        {
            base.OnControlSizeChanged();
            ReadOnlyControl!.Width = ReadOnlyLabel.Width + ReadOnlyPictureSize;
            ReadOnlyControl.Height = ReadOnlyPictureSize + 2;
        }

        protected override void OnControlFontChanged()
        {
            base.OnControlFontChanged();
            ReadOnlyLabel.Font = ReadOnlyControl!.Font;
        }

        protected override void OnControlValueChanged(object? value) => 
            ReadOnlyPicture.Image = CheckBox.Checked 
                ? OxIcons.Tick 
                : (Image)OxIcons.Cross;

        protected override void OnControlTextChanged(string? text)
        {
            if (ReadOnlyControl is null)
                return;

            ReadOnlyLabel.Text = Control.Text;
            OnControlFontChanged();
        }

        public override object? MaximumValue
        {
            get => CheckBox.Enabled;
            set
            {
                if (DAO.IntValue(value) == 0)
                    CheckBox.Checked = false;

                CheckBox.Enabled = DAO.IntValue(value) > 0;
            }
        }
    }
}