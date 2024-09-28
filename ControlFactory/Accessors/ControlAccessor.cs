using OxDAOEngine.ControlFactory.Context;
using OxDAOEngine.ControlFactory.ValueAccessors;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Types;
using OxLibrary;
using OxLibrary.Controls;

namespace OxDAOEngine.ControlFactory.Accessors
{
    public abstract class ControlAccessor<TField, TDAO> : IControlAccessor
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        private Control control = default!;
        private ValueAccessor valueAccessor = default!;
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
                if (valueChangeHandler != null)
                    UnAssignValueChangeHanlderToControl(valueChangeHandler);

                valueChangeHandler = null;
            }
        }

        protected virtual void AssignValueChangeHanlderToControl(EventHandler? value) { }
        protected virtual void UnAssignValueChangeHanlderToControl(EventHandler? value) { }

        protected object? GetValue() => 
            valueAccessor.GetValue();

        protected void SetValue(object? value)
        {
            valueAccessor.SetValue(value);
            OnControlValueChanged(value);
        }

        protected virtual void OnControlValueChanged(object? value)
        {
            if (ReadOnlyControl != null)
                OnControlTextChanged(TypeHelper.Name(value));
        }

        public virtual bool IsEmpty => 
            Value == null || Value.ToString() == string.Empty;

        protected virtual void InitControl()
        {
            Control.Font = EngineStyles.DefaultFont;
            Control.Height = EngineStyles.DefaultControlHeight;

            Control.SizeChanged += ControlSizeChangeHandler;
            Control.LocationChanged += ControlLocationChangeHandler;
            Control.TextChanged += ControlTextChangedHandler;
            Control.ParentChanged += ControlParentChangedHandler;
            Control.BackColorChanged += ControlBackColorChangedHandler;
            Control.FontChanged += ControlFontChangedHandler;
            Control.ForeColorChanged += ControlForeColorChangedHandler;
            Control.VisibleChanged += ControlVisibleChangedHandler;
            Control.DockChanged += ControlDockChangedHandler;
        }

        private void ControlDockChangedHandler(object? sender, EventArgs e)
        {
            if (ReadOnlyControl != null)
                ReadOnlyControl.Dock = Control.Dock;
        }

        private void ControlVisibleChangedHandler(object? sender, EventArgs e) => OnControlVisibleChanged();

        protected virtual void OnControlVisibleChanged()
        {
            if (ReadOnlyControl != null)
                ReadOnlyControl.Visible = Visible && ReadOnly && !Control.Visible;
        }

        private void ControlForeColorChangedHandler(object? sender, EventArgs e) => OnControlForeColorChanged();

        protected virtual void OnControlForeColorChanged()
        {
            if (ReadOnlyControl != null)
                ReadOnlyControl.ForeColor = Control.ForeColor;
        }

        private void ControlFontChangedHandler(object? sender, EventArgs e) => OnControlFontChanged();

        protected virtual void OnControlFontChanged()
        {
            if (ReadOnlyControl != null)
                ReadOnlyControl.Font = new Font(
                    Control.Font.FontFamily,
                    Control.Font.Size + (Control.Text == string.Empty ? 0 : 1),
                    Control.Font.Style 
                        | (Control.Text == string.Empty
                            ? FontStyle.Italic
                            : FontStyle.Regular
                        )
                );
        }

        private void ControlBackColorChangedHandler(object? sender, EventArgs e) => OnControlBackColorChanged();

        protected virtual void OnControlBackColorChanged()
        {
            if (ReadOnlyControl != null)
                try
                {
                    ReadOnlyControl.BackColor = new OxColorHelper(Control.BackColor).Lighter();
                }
                catch 
                { 
                }
        }

        private void ControlParentChangedHandler(object? sender, EventArgs e) => OnControlParentChanged();

        protected virtual void OnControlParentChanged()
        {
            if (ReadOnlyControl != null)
                ReadOnlyControl.Parent = Control.Parent;
        }

        private void ControlTextChangedHandler(object? sender, EventArgs e) =>
            OnControlTextChanged(Control.Text);

        protected virtual void OnControlTextChanged(string? text)
        {
            if (ReadOnlyControl == null)
                return;

            ReadOnlyControl.Text = 
                string.IsNullOrEmpty(text)
                    ? "Empty" 
                    : text;

            OnControlFontChanged();
        }

        private void ControlLocationChangeHandler(object? sender, EventArgs e) => OnControlLocationChanged();

        protected virtual void OnControlLocationChanged()
        {
            if (ReadOnlyControl == null)
                return;
            
            ReadOnlyControl.Left = Control.Left;
            OxControlHelper.AlignByBaseLine(Control, ReadOnlyControl);
            ReadOnlyControl.Top += 1;
        }

        private void ControlSizeChangeHandler(object? sender, EventArgs e) => OnControlSizeChanged();

        protected virtual void OnControlSizeChanged()
        {
            if (ReadOnlyControl == null)
                return;

            ReadOnlyControl.Width = Control.Width;
            ReadOnlyControl.Height = Control.Height;
        }

        private bool readOnly = false;

        private bool visible = true;
        protected abstract ValueAccessor CreateValueAccessor();
        protected abstract Control CreateControl();

        protected virtual Control? CreateReadOnlyControl() => new OxLabel()
        { 
            Font = new Font(Control.Font.FontFamily, Control.Font.Size+1)
        };

        protected Control? ReadOnlyControl;

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
            valueAccessor = CreateValueAccessor();
            valueAccessor.SetControl(control);
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
            return IsEmpty || tValue == null ? default! : tValue;
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
            Value != null && Value.ToString() != null 
                ? Value.ToString()!
                : string.Empty;

        public string SingleStringValue =>
            StringValue.Replace("\n", " ").Replace("  ", " ");

        public Guid GuidValue => 
            Guid.Parse(StringValue);

        public Control Control => control;

        public bool Enabled
        {
            get => Control.Enabled;
            set => Control.Enabled = value;
        }

        public bool ReadOnly
        {
            get => GetReadOnly();
            set => SetReadOnly(value);
        }

        protected virtual void SetReadOnly(bool value)
        {
            readOnly = value;

            if (ReadOnlyControl != null)
            {
                Control.Visible = visible && !readOnly;
                ReadOnlyControl.Visible = visible && readOnly;
            }
            else Control.Enabled = !readOnly;
        }

        protected virtual bool GetReadOnly() =>
            readOnly;

        public int Left
        {
            get => Control.Left;
            set => Control.Left = value;
        }

        public int Right
        {
            get => Left + Width;
            set => Left = value - Width;
        }

        public int Top
        {
            get => Control.Top;
            set => Control.Top = value;
        }

        public int Bottom
        {
            get => Top + Height;
            set => Top = value - Height;
        }

        public int Width
        {
            get => Control.Width;
            set => Control.Width = value;
        }

        public int Height
        {
            get => Control.Height;
            set => Control.Height = value;
        }

        public Control? Parent
        {
            get => Control.Parent;
            set => Control.Parent = value;
        }

        public DockStyle Dock
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
        public bool Visible 
        { 
            get => GetVisible();
            set => SetVisible(value);
        }

        IAccessorContext IControlAccessor.Context => Context;

        protected virtual bool GetVisible() => visible;

        protected virtual void SetVisible(bool value)
        {
            visible = value;
            Control.Visible = visible && !readOnly;
        }

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
    }
}