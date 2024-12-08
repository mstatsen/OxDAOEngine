using OxDAOEngine.Data;
using OxDAOEngine.Data.Sorting.Types;
using OxLibrary.Interfaces;

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