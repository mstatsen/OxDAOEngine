using OxDAOEngine.ControlFactory.Accessors;
using OxDAOEngine.ControlFactory.Initializers;
using OxDAOEngine.Data.Fields;

namespace OxDAOEngine.ControlFactory.Context
{
    public interface IAccessorContext
    {
        string Name { get; }
        IInitializer? Initializer { get; set; }
        FieldType FieldType { get; }
        ControlScope Scope { get; }
        bool IsQuickFilter { get; }
        bool IsBatchUpdate { get; }
        bool IsView { get; }
        void InitControl(IControlAccessor accessor);
        object? AdditionalContext { get; set; }
        bool MultipleValue { get; set; }
        IAccessorContext SetInitializer(IInitializer initializer);
        EventHandler? InitializerChanged { get; set; }
    }
}