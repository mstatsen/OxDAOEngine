using OxLibrary.Panels;

namespace OxDAOEngine.Summary
{
    public interface ISummaryPanel : IOxCard
    {
        void AlignAccessors();
        void ClearAccessors();
        void CalcPanelSize();
        void FillAccessors();
    }
}