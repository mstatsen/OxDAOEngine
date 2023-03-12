using OxLibrary.Panels;
using OxXMLEngine.ControlFactory.Context;
using OxXMLEngine.Data;

namespace OxXMLEngine.ControlFactory.Controls
{
    public abstract class CustomControl<TField, TDAO, TItem> : OxPane, 
        ICustomControl<TField, TDAO>, IColoredCustomControl
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
        where TItem : DAO
    {
        public CustomControl<TField, TDAO, TItem> Init(IBuilderContext<TField, TDAO> context)
        {
            Context = context;
            InitComponents();
            RecalcControls();
            SizeChanged += SizeChangedHandler;
            return this;
        }

        public IBuilderContext<TField, TDAO> Context { get; private set; } = default!;

        public EventHandler? ValueChangeHandler;
        protected abstract Control GetControl();
        protected abstract void SetValue(TItem value);
        protected abstract TItem GetValue();
        protected abstract void InitComponents();

        public TItem Value
        {
            get => GetValue();
            set => SetValue(value);
        }

        public Control Control => GetControl();

        protected virtual Color GetControlColor() => 
            Control == null 
                ? BackColor 
                : Control.BackColor;

        protected virtual void SetControlColor(Color value)
        {
            if (Control != null)
                Control.BackColor = value;
        }

        public Color ControlColor
        {
            get => GetControlColor();
            set => SetControlColor(value);
        }

        private void SizeChangedHandler(object? sender, EventArgs e) => 
            RecalcControls();

        protected virtual void RecalcControls() { }

        public bool ReadOnly
        {
            get => GetReadOnly();
            set => SetReadOnly(value);
        }

        protected abstract bool GetReadOnly();
        protected abstract void SetReadOnly(bool value);
    }
}