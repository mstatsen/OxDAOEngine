using OxLibrary;
using OxLibrary.Controls;
using OxLibrary.Geometry;
using OxLibrary.Interfaces;
using OxLibrary.Panels;
using OxDAOEngine.ControlFactory.Context;
using OxDAOEngine.ControlFactory.ValueAccessors;
using OxDAOEngine.Data;

namespace OxDAOEngine.ControlFactory.Accessors;

public class CheckBoxAccessor<TField, TDAO> : ControlAccessor<TField, TDAO>
    where TField : notnull, Enum
    where TDAO : RootDAO<TField>, new()
{
    public CheckBoxAccessor(IBuilderContext<TField, TDAO> context) : base(context) { }

    protected override IOxControl CreateControl()
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

    private readonly short ReadOnlyPictureSize = 16;

    protected override IOxControl? CreateReadOnlyControl()
    {
        OxPanel readOnlyControl = new();
        ReadOnlyLabel = new OxLabel
        {
            Parent = readOnlyControl
        };
        ReadOnlyPicture.Parent = readOnlyControl;
        ReadOnlyPicture.Height = ReadOnlyPictureSize;
        ReadOnlyPicture.Width = ReadOnlyPictureSize;
        ReadOnlyPicture.MinimumSize = new(ReadOnlyPictureSize, ReadOnlyPictureSize);
        ReadOnlyPicture.Top = 0;
        ReadOnlyPicture.Left = OxSh.Sub(readOnlyControl.Width, ReadOnlyPictureSize);
        ReadOnlyPicture.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
        ReadOnlyPicture.PictureSize = ReadOnlyPictureSize;
        ReadOnlyPicture.Image = OxIcons.Tick;
        return readOnlyControl;
    }

    protected override void OnControlSizeChanged()
    {
        base.OnControlSizeChanged();
        ReadOnlyControl!.Width = OxSh.Sub(ReadOnlyLabel.Width, ReadOnlyPictureSize);
        ReadOnlyControl.Height = OxSh.Add(ReadOnlyPictureSize, 2);
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
            if (DAO.IntValue(value) is 0)
                CheckBox.Checked = false;

            CheckBox.SetEnabled(DAO.IntValue(value) > 0);
        }
    }
}