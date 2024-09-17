namespace OxDAOEngine.Grid
{
    public static class GridUsageHelper
    {
        public static bool IsReadOnly(GridUsage usage) =>
            usage switch
            {
                GridUsage.Edit => false,
                _ => true
            };
    }
}