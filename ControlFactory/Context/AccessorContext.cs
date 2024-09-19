using OxDAOEngine.ControlFactory.Accessors;
using OxDAOEngine.ControlFactory.Initializers;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Fields;
using OxDAOEngine.Data.Types;

namespace OxDAOEngine.ControlFactory.Context
{
    public class AccessorContext<TField, TDAO> : IBuilderContext<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public AccessorContext(ControlBuilder<TField, TDAO> builder, string name, FieldType fieldType)
        {
            Builder = builder;
            Name = name;
            FieldType = fieldType;
        }

        public FieldType FieldType { get; internal set; }

        public ControlBuilder<TField, TDAO> Builder { get; internal set; }

        public string Name { get; internal set; }

        public IInitializer? Initializer { get; set; }

        public ControlScope Scope => Builder.Scope;

        public object? AdditionalContext { get; set; }

        public bool IsQuickFilter => scopeHelper.IsQuickFilter(Scope);

        public bool IsBatchUpdate => Scope == ControlScope.BatchUpdate;

        public bool IsView => scopeHelper.IsView(Scope);

        private readonly ControlScopeHelper scopeHelper = TypeHelper.Helper<ControlScopeHelper>();

        public IBuilderContext<TField, TDAO> SetInitializer(IInitializer initializer)
        {
            Initializer = initializer;
            InitializerChanged?.Invoke(this, EventArgs.Empty);
            return this;
        }

        IAccessorContext IAccessorContext.SetInitializer(IInitializer initializer) =>
            SetInitializer(initializer);

        public void InitControl(IControlAccessor accessor) =>
            Initializer?.InitControl(accessor.Control);
        public EventHandler? InitializerChanged { get; set; }
        public bool MultipleValue { get; set; }
        public TDAO? Item { get; set; }
    }
}