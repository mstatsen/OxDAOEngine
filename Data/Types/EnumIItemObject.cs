namespace OxDAOEngine.Data.Types
{
    public class EnumItemObject<T> : IComparable 
        where T : Enum
    {
        public T Value = default!;

        public virtual int CompareTo(object? obj)
        {
            T? otherValue = default;

            if (obj is EnumItemObject<T> otherEnum)
                otherValue = otherEnum.Value;

            return Value.CompareTo(otherValue);
        }

        public override string? ToString() => 
            TypeHelper.Helper(this).UseShortNameForControl ? TypeHelper.ShortName(Value) : TypeHelper.Name(Value);
    }
}
