namespace OxDAOEngine.Data.Types
{
    public interface IDependedHelper<TDepended>
        where TDepended : Enum
    {
        List<TDepended> DependedList(object value);

        object? DependsOnValue(TDepended? value);
        U? DependsOnValue<U>(TDepended? value) where U : Enum;
    }

    public interface IDependedHelper<TField, TDepended> : IDependedHelper<TDepended>
        where TField : notnull, Enum
        where TDepended : notnull, Enum
    {
        List<TDepended> DependedList(TField field, object value);
    }
}
