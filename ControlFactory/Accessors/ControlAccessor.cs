using OxDAOEngine.ControlFactory.Context;
using OxDAOEngine.ControlFactory.ValueAccessors;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Types;
using OxLibrary;
using OxLibrary.Controls;
using OxLibrary.Geometry;
using OxLibrary.Interfaces;

namespace OxDAOEngine.ControlFactory.Accessors;

public abstract class ControlAccessor<TField, TDAO> : IControlAccessor
    where TField : notnull, Enum
    where TDAO : RootDAO<TField>, new()
{
    private IOxControl control = default!;
    protected ValueAccessor ValueAccessor { get; private set; } = default!;
    private EventHandler? valueChangeHandler;

    public event EventHandler? ValueChangeHandler
    {
        add
        {
            valueChangeHandler = value;
            AssignValueChangeHanlderToControl(value);
        } 
        remove
        {
            if (valueChangeHandler is not null)
                UnAssignValueChangeHanlderToControl(valueChangeHandler);

            valueChangeHandler = null;
        }
    }

    protected virtual void AssignValueChangeHanlderToControl(EventHandler? value) { }
    protected virtual void UnAssignValueChangeHanlderToControl(EventHandler? value) { }

    protected object? GetValue() => 
        ValueAccessor.GetValue();

    public virtual object? ObjectValue => Value;

    protected void SetValue(object? value)
    {
        ValueAccessor.SetValue(value);
        OnControlValueChanged(value);
    }

    protected virtual void OnControlValueChanged(object? value)
    {
        if (ReadOnlyControl is not null)
            OnControlTextChanged(TypeHelper.Name(value));
    }

    public virtual bool IsEmpty => 
        Value is null 
        || Value.ToString() is null 
        || Value.ToString()!.Equals(string.Empty);

    protected virtual void InitControl()
    {
        Control.Font = OxStyles.DefaultFont;
        Control.Height = EngineStyles.DefaultControlHeight;
        Control.SizeChanged += ControlSizeChangeHandler;
        Control.LocationChanged += ControlLocationChangeHandler;
        Control.ParentChanged += ControlParentChangedHandler;
        Control.TextChanged += ControlTextChangedHandler;
        Control.BackColorChanged += ControlBackColorChangedHandler;
        Control.FontChanged += ControlFontChangedHandler;
        Control.ForeColorChanged += ControlForeColorChangedHandler;
        Control.VisibleChanged += ControlVisibleChangedHandler;
        Control.DockChanged += ControlDockChangedHandler;
    }

    private void ControlDockChangedHandler(object? sender, EventArgs e)
    {
        if (ReadOnlyControl is not null)
            ReadOnlyControl.Dock = Control.Dock;
    }

    private void ControlVisibleChangedHandler(object? sender, EventArgs e) =>
        OnControlVisibleChanged();

    protected virtual void OnControlVisibleChanged() =>
        ReadOnlyControl?.SetVisible(
            IsVisible
            && IsReadOnly
            && !Control.IsVisible
        );

    private void ControlForeColorChangedHandler(object? sender, EventArgs e) =>
        OnControlForeColorChanged();

    protected virtual void OnControlForeColorChanged()
    {
        if (ReadOnlyControl is not null)
            ReadOnlyControl.ForeColor = Control.ForeColor;
    }

    private void ControlFontChangedHandler(object? sender, EventArgs e) =>
        OnControlFontChanged();

    protected virtual void OnControlFontChanged()
    {
        if (ReadOnlyControl is not null)
            ReadOnlyControl.Font = new(
                Control.Font.FontFamily,
                Control.Font.Size + (Control.Text.Equals(string.Empty) ? 0 : 1),
                Control.Font.Style 
                    | (Control.Text.Equals(string.Empty)
                        ? FontStyle.Italic
                        : FontStyle.Regular
                    )
            );
    }

    private void ControlBackColorChangedHandler(object? sender, EventArgs e) =>
        OnControlBackColorChanged();

    protected virtual void OnControlBackColorChanged()
    {
        if (ReadOnlyControl is not null 
            && Parent is not null)
            ReadOnlyControl.BackColor = Parent.BackColor;
    }

    private void ControlParentChangedHandler(object? sender, EventArgs e) =>
        OnControlParentChanged();

    protected virtual void OnControlParentChanged()
    {
        if (ReadOnlyControl is not null)
            ReadOnlyControl.Parent = Control.Parent;
    }

    private void ControlTextChangedHandler(object? sender, EventArgs e) =>
        OnControlTextChanged(Control.Text);

    protected virtual void OnControlTextChanged(string? text)
    {
        if (ReadOnlyControl is null)
            return;

        ReadOnlyControl.Text = 
            string.IsNullOrEmpty(text)
                ? "Empty" 
                : text;

        OnControlFontChanged();
    }

    private void ControlLocationChangeHandler(object? sender, EventArgs e) =>
        OnControlLocationChanged();

    protected virtual void OnControlLocationChanged()
    {
        if (ReadOnlyControl is null)
            return;
        
        ReadOnlyControl.Left = Control.Left;
        OxControlHelper.AlignByBaseLine(Control, ReadOnlyControl);
        ReadOnlyControl.Top += 1;
    }

    private void ControlSizeChangeHandler(object? sender, EventArgs e) =>
        OnControlSizeChanged();

    protected virtual void OnControlSizeChanged()
    {
        if (ReadOnlyControl is null)
            return;

        ReadOnlyControl.Width = Control.Width;
        ReadOnlyControl.Height = Control.Height;
    }

    private OxBool readOnly = OxB.F;

    private OxBool visible = OxB.T;
    protected abstract ValueAccessor CreateValueAccessor();
    protected abstract IOxControl CreateControl();

    protected virtual IOxControl? CreateReadOnlyControl() => new OxTextBox()
    { 
        Font = new(Control.Font.FontFamily, Control.Font.Size+1),
        BorderStyle = BorderStyle.None,
        Multiline = true,
        ReadOnly = true
    };

    public IOxControl? ReadOnlyControl { get; private set; }

    protected virtual void AfterControlsCreated() { }

    public readonly IBuilderContext<TField, TDAO> Context;

    public ControlAccessor(IBuilderContext<TField, TDAO> context)
    {
        Context = context;
        Context.InitializerChanged = (s, e) => RenewControl(true);

        if (AutoInit)
            Init();
    }

    protected virtual bool AutoInit => true;

    public ControlAccessor<TField, TDAO> Init()
    {
        control = CreateControl();
        ReadOnlyControl = CreateReadOnlyControl();
        AfterControlsCreated();
        InitControl();
        ValueAccessor = CreateValueAccessor();
        ValueAccessor.SetControl(control);
        Context.InitControl(this);
        AfterInitControl();
        return this;
    }

    protected virtual void AfterInitControl() { }

    public object? Value
    {
        get => GetValue();
        set => SetValue(value);
    }

    public T? DAOValue<T>()
        where T : DAO =>
        IsEmpty ? null : (T?)GetValue();

    public T EnumValue<T>()
        where T : Enum
    {
        T? tValue = (T?)GetValue();
        return IsEmpty || tValue is null 
            ? default! 
            : tValue;
    }

    public bool BoolValue => 
        bool.TryParse(StringValue, out bool result) && result;

    public int IntValue
    {
        get
        {
            switch (Value)
            {
                case bool boolean:
                    return boolean ? 1 : 0;
                default:
                    if (int.TryParse(StringValue, out int result))
                        return result;
                    else return 0;
            }
        }
    }

    public string StringValue => 
        Value is not null 
        && Value.ToString() is not null 
            ? Value.ToString()!
            : string.Empty;

    public string SingleStringValue =>
        StringValue.Replace("\n", " ").Replace("  ", " ");

    public Guid GuidValue => 
        Guid.Parse(StringValue);

    public IOxControl Control => control;

    public OxBool Enabled
    {
        get => Control.Enabled;
        set => Control.Enabled = value;
    }

    public bool IsEnabled =>
        OxB.B(Enabled);

    public void SetEnabled(bool value) =>
        Enabled = OxB.B(value);

    public OxBool ReadOnly
    {
        get => GetReadOnly();
        set => SetReadOnly(value);
    }

    protected virtual void SetReadOnly(OxBool value)
    {
        readOnly = value;

        if (ReadOnlyControl is not null)
        {
            Control.SetVisible(IsVisible && !IsReadOnly);
            ReadOnlyControl.SetVisible(IsVisible && IsReadOnly);
        }
        else Control.Enabled = OxB.Not(readOnly);
    }

    protected virtual OxBool GetReadOnly() =>
        readOnly;

    public bool IsReadOnly =>
        OxB.B(ReadOnly);

    public void SetReadOnly(bool value) =>
        ReadOnly = OxB.B(value);

    public short Left
    {
        get => Control.Left;
        set => Control.Left = value;
    }

    public short Right
    {
        get => OxSh.Add(Left, Width);
        set => Left = OxSh.Sub(value, Width);
    }

    public short Top
    {
        get => Control.Top;
        set => Control.Top = value;
    }

    public short Bottom
    {
        get => OxSh.Add(Top, Height);
        set => Top = OxSh.Sub(value, Height);
    }

    public short Width
    {
        get => Control.Width;
        set => Control.Width = value;
    }

    public short Height
    {
        get => Control.Height;
        set => Control.Height = value;
    }

    public IOxBox? Parent
    {
        get => Control.Parent;
        set => Control.Parent = value;
    }

    public OxDock Dock
    {
        get => Control.Dock;
        set => Control.Dock = value;
    }

    public AnchorStyles Anchor
    {
        get => Control.Anchor;
        set => Control.Anchor = value;
    }

    public string Text
    {
        get => Control.Text;
        set => Control.Text = value;
    }

    public PlacedControl<TField> LayoutControl(ControlLayout<TField> layout) =>
        layout.ApplyLayout(Control);

    public virtual void ClearValueConstraints() { }

    public virtual object? MaximumValue { get; set; }
    public virtual object? MinimumValue { get; set; }
    public OxBool Visible 
    { 
        get => GetVisible();
        set => SetVisible(value);
    }

    IAccessorContext IControlAccessor.Context => Context;

    protected virtual OxBool GetVisible() => visible;

    protected virtual void SetVisible(OxBool value)
    {
        visible = value;
        Control.SetVisible(IsVisible && !IsReadOnly);
    }

    public bool IsVisible =>
        OxB.B(Visible);

    public void SetVisible(bool value) =>
        Visible = OxB.B(value);

    public abstract void Clear();

    public virtual void RenewControl(bool hardReset = false)
    {
        object? oldValue = Value;

        if (hardReset)
            InitControl();

        Context.InitControl(this);
        AfterInitControl();
        Value = oldValue;
    }

    public void SuspendLayout() => Control?.Parent?.SuspendLayout();

    public void ResumeLayout() => Control?.Parent?.ResumeLayout();

    IControlAccessor IControlAccessor.Init() => Init();

    public virtual void SetDefaultValue() =>
        Clear();

    public bool CenteredReadonlyText
    {
        get => ReadOnlyControl is OxTextBox textBox 
            && textBox.TextAlign is HorizontalAlignment.Center;
        set
        {
            if (ReadOnlyControl is OxTextBox textBox)
                textBox.TextAlign = value 
                    ? HorizontalAlignment.Center 
                    : HorizontalAlignment.Left;
        }
    }
}