using OxXMLEngine.ControlFactory.Context;
using OxXMLEngine.ControlFactory.ValueAccessors;
using OxXMLEngine.Data;

namespace OxXMLEngine.ControlFactory.Accessors
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

        protected void SetValue(object? value) => 
            valueAccessor.SetValue(value);

        public virtual bool IsEmpty => 
            Value == null || Value.ToString() == string.Empty;

        protected virtual void InitControl()
        {
            Control.Font = EngineStyles.DefaultFont;
            Control.Height = EngineStyles.DefaultControlHeight;
        }

        protected abstract ValueAccessor CreateValueAccessor();
        protected abstract Control CreateControl();

        protected virtual void AfterControlCreated() { }

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
            AfterControlCreated();
            InitControl();
            valueAccessor = CreateValueAccessor();
            valueAccessor.SetControl(control);
            Context.InitControl(this);
            return this;
        }

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

        protected virtual void SetReadOnly(bool value) =>
            Control.Enabled = !value;

        protected virtual bool GetReadOnly() =>
            !Control.Enabled;

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

        protected virtual bool GetVisible() => 
            Control.Visible;

        protected virtual void SetVisible(bool value) => 
            Control.Visible = value;

        public abstract void Clear();

        public virtual void RenewControl(bool hardReset = false)
        {
            object? oldValue = Value;

            if (hardReset)
                InitControl();

            Context.InitControl(this);
            Value = oldValue;
        }

        public void SuspendLayout() => Control?.Parent?.SuspendLayout();

        public void ResumeLayout() => Control?.Parent?.ResumeLayout();

        IControlAccessor IControlAccessor.Init() => Init();
    }
}