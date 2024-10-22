using OxLibrary.Panels;
using OxDAOEngine.Data;
using OxDAOEngine.Data.Sorting.Types;

namespace OxDAOEngine.ControlFactory.Controls.Sorting
{
    public interface ISortingPanel : IOxFrameWithHeader
    {
        void ResetToDefault();
        void RenewControls();
        SortingVariant Variant { get; }
        DAOEntityEventHandler? ExternalChangeHandler { get; set; }
    }
}