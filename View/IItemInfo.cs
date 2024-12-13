using OxLibrary.Panels;
using OxDAOEngine.Data;
using OxDAOEngine.ControlFactory;
using OxLibrary;

namespace OxDAOEngine.View
{
    public interface IItemInfo<TField, TDAO> : IItemView<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    { 
        OxBool Expanded { get; set; }
        bool IsExpanded { get; }
        void SetExpanded(bool value);
        OxBool Pinned { get; set; }
        bool IsPinned { get; }
        void SetPinned(bool value);
        OxPanel Sider { get; }
        OxBool SiderEnabled { get; set; }
        void SetSiderEnabled(bool value);
        bool IsSiderEnabled { get; }
        void ApplySettings();
        void SaveSettings();
        new FunctionalPanelVisible Visible { get; set; }
        void ScrollToTop();
    }
}
