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
        public AccessorContext(ControlBuilder<TField, TDAO> builder, string key, string name, FieldType fieldType)
        {
            Builder = builder;
            Key = key;
            Name = name;
            FieldType = fieldType;
        }

        public FieldType FieldType { get; internal set; }

        public ControlBuilder<TField, TDAO> Builder { get; internal set; }

        public string Name { get; internal set; }
        public string Key { get; internal set; }

        public IInitializer? Initializer { get; set; }

        public ControlScope Scope => Builder.Scope;

        public object? AdditionalContext { get; set; }

        private readonly ControlScopeHelper ScopeHelper = 
            TypeHelper.Helper<ControlScopeHelper>();

        public bool IsQuickFilter => 
            ScopeHelper.IsQuickFilter(Scope);

        public bool IsBatchUpdate => 
            Scope is ControlScope.BatchUpdate;

        public bool IsCategory => 
            Scope is ControlScope.Category;

        public bool IsView => ScopeHelper.IsView(Scope);

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

        public IControlAccessor Accessor(string name, FieldType fieldType, object? additionalContext = null) => 
            Builder.Accessor(name, fieldType, additionalContext);

        public IControlAccessor Accessor(TField field) => 
            Builder.Accessor(field);
    }
}