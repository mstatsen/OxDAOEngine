using OxDAOEngine.ControlFactory.Context;
using OxDAOEngine.Data;
using OxLibrary;
using OxLibrary.Interfaces;

namespace OxDAOEngine.ControlFactory.Accessors
{
    public interface IControlAccessor
    {
        event EventHandler ValueChangeHandler;
        IAccessorContext Context { get; }
        IControlAccessor Init();
        IOxControl Control { get; }
        void ClearValueConstraints();
        void Clear();
        void RenewControl(bool hardReset = false);
        void SuspendLayout();
        void ResumeLayout();
        T? DAOValue<T>() where T : DAO;
        T? EnumValue<T>() where T : Enum;
        object? Value { get; set; }
        object? ObjectValue { get; }
        bool BoolValue { get; }
        int IntValue { get; }
        string StringValue { get; }
        bool IsEmpty { get; }
        string Text { get; set; }
        object? MaximumValue { get; set; }
        object? MinimumValue { get; set; }
        IOxBox? Parent { get; set; }
        bool Visible { get; set; }
        bool Enabled { get; set; }
        bool ReadOnly { get; set; }
        short Left { get; set; }
        short Right { get; set; }
        short Top { get; set; }
        short Bottom { get; set; }
        short Width { get; set; }
        short Height { get; set; }
        OxDock Dock { get; set; }
        AnchorStyles Anchor { get; set; }
        string SingleStringValue { get; }
        Guid GuidValue { get; }
        void SetDefaultValue();

        IOxControl? ReadOnlyControl { get; }
    }
}
