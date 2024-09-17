using OxDAOEngine.Data.Types;

namespace OxDAOEngine.Statistic
{
    public class StatisticTypeHelper : AbstractTypeHelper<StatisticType>
    {
        public override StatisticType EmptyValue() => StatisticType.Total;

        public override string GetName(StatisticType value) =>
            value switch
            {
                StatisticType.Total => "Total",
                StatisticType.Visible => "Visible",
                StatisticType.Selected => "Selected",
                StatisticType.Modified => "Modified",
                StatisticType.Added => "Added",
                StatisticType.Deleted => "Deleted",
                _ => string.Empty
            };

        public DockStyle Dock(StatisticType type) =>
            type switch
            {
                StatisticType.Modified or
                StatisticType.Added or
                StatisticType.Deleted =>
                    DockStyle.Right,
                _ =>
                    DockStyle.Left
            };

        public int Width(StatisticType type) =>
            type switch
            {
                StatisticType.Total =>
                    170,
                StatisticType.Category =>
                    200,
                _ =>
                    120
            };
    }
}