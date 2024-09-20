namespace OxDAOEngine.Data.Decorator
{
    internal class DecoratorCache<TField, TDAO> : Dictionary<DecoratorType, Decorator<TField, TDAO>>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public Decorator<TField, TDAO>? GetDecorator(DecoratorType type)
        {
            if (!TryGetValue(type, out var decorator))
                decorator = null;

            return decorator;
        }

        public void SetDecorator(DecoratorType type, Decorator<TField, TDAO> decorator) => 
            this[type] = decorator;
    }
}