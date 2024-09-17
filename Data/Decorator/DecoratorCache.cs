namespace OxDAOEngine.Data.Decorator
{
    internal class DecoratorCache<TField, TDAO> : Dictionary<TDAO, Dictionary<DecoratorType, Decorator<TField, TDAO>>>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    { 
        private Dictionary<DecoratorType, Decorator<TField, TDAO>> Decorators(TDAO item)
        { 
            if (!TryGetValue(item, out var decorators))
            {
                Add(item, new Dictionary<DecoratorType, Decorator<TField, TDAO>>());
                decorators = this[item];
            }

            return decorators;
        }

        public Decorator<TField, TDAO>? GetDecorator(TDAO item, DecoratorType type)
        {
            Decorators(item).TryGetValue(type, out var decorator);
            return decorator;
        }

        public void SetDecorator(TDAO item, DecoratorType type, Decorator<TField, TDAO> decorator) => 
            Decorators(item)[type] = decorator;
    }
}