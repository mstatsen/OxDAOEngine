using OxLibrary;
using OxLibrary.Panels;
using OxDAOEngine.ControlFactory.Context;
using OxDAOEngine.Data;

namespace OxDAOEngine.ControlFactory.Controls
{
    public abstract class CustomControl<TField, TDAO, TItem> : OxPanel, 
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
            return this;
        }

        public override bool OnSizeChanged(SizeChangedEventArgs e)
        {
            base.OnSizeChanged(e);

            if (e.Changed)
                RecalcControls();

            return e.Changed;
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
            Control is null 
                ? BackColor 
                : Control.BackColor;

        protected virtual void SetControlColor(Color value)
        {
            if (Control is not null)
                Control.BackColor = value;
        }

        public Color ControlColor
        {
            get => GetControlColor();
            set => SetControlColor(value);
        }

        protected virtual void RecalcControls() { }

        public bool ReadOnly
        {
            get => GetReadOnly();
            set => SetReadOnly(value);
        }

        protected abstract bool GetReadOnly();
        protected abstract void SetReadOnly(bool value);

        private EventHandler? itemEdited;

        public EventHandler? ItemEdited
        {
            get => itemEdited;
            set => SetItemEdited(value);
        }

        protected virtual void SetItemEdited(EventHandler? value) =>
            itemEdited = value;

        public virtual object? PrepareValueToReadOnly(TItem? value) =>
            value;

        public TDAO? OwnerDAO { get; set; }

        public virtual ReadonlyMode ReadonlyMode => ReadonlyMode.ViewAsReadonly;
    }
}