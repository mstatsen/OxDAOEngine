using OxLibrary;

namespace OxDAOEngine.Grid
{
    public static class GridUsageHelper
    {
        public static OxBool IsReadOnly(GridUsage usage) =>
            OxB.B(usage is not GridUsage.Edit);
    }
}