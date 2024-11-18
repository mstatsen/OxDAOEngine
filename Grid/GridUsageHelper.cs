namespace OxDAOEngine.Grid
{
    public static class GridUsageHelper
    {
        public static bool IsReadOnly(GridUsage usage) =>
            usage is not GridUsage.Edit;
    }
}