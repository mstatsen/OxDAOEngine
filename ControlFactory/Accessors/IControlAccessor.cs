using OxDAOEngine.ControlFactory.Context;
using OxDAOEngine.Data;

namespace OxDAOEngine.ControlFactory.Accessors
{
    public interface IControlAccessor
    {
        event EventHandler ValueChangeHandler;
        IAccessorContext Context { get; }
        IControlAccessor Init();
        Control Control { get; }
        void ClearValueConstraints();
        void Clear();
        void RenewControl(bool hardReset = false);
        void SuspendLayout();
        void ResumeLayout();
        T? DAOValue<T>() where T : DAO;
        T? EnumValue<T>() where T : Enum;
        object? Value { get; set; }
        bool BoolValue { get; }
        int IntValue { get; }
        string StringValue { get; }
        bool IsEmpty { get; }
        string Text { get; set; }
        object? MaximumValue { get; set; }
        object? MinimumValue { get; set; }
        Control? Parent { get; set; }
        bool Visible { get; set; }
        bool Enabled { get; set; }
        bool ReadOnly { get; set; }
        int Left { get; set; }
        int Right { get; set; }
        int Top { get; set; }
        int Bottom { get; set; }
        int Width { get; set; }
        int Height { get; set; }
        DockStyle Dock { get; set; }
        AnchorStyles Anchor { get; set; }
        string SingleStringValue { get; }
        Guid GuidValue { get; }
        void SetDefaultValue();

        Control? ReadOnlyControl { get; }
    }
}
