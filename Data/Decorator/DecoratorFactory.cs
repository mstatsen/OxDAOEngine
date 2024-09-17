using OxDAOEngine.ControlFactory;
using OxDAOEngine.Data.Types;

namespace OxDAOEngine.Data.Decorator
{
    public abstract class DecoratorFactory<TField, TDAO>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        private readonly DecoratorCache<TField, TDAO> cache = new();

        public Decorator<TField, TDAO> Decorator(ControlScope scope, TDAO dao) =>
            Decorator(
                TypeHelper.Helper<DecoratorTypeHelper>().DecoratorTypeByScope(scope),
                dao
            );

        public Decorator<TField, TDAO> Decorator(DecoratorType type, TDAO dao)
        {
            Decorator<TField, TDAO>? decorator = cache.GetDecorator(dao, type);

            if (decorator == null)
            {
                decorator =
                    type switch
                    {
                        DecoratorType.Simple => Simple(dao),
                        DecoratorType.Card => Card(dao),
                        DecoratorType.Icon => Icon(dao),
                        DecoratorType.FullInfo => FullInfo(dao),
                        DecoratorType.Html => HTML(dao),
                        _ => Table(dao),
                    };

                cache.SetDecorator(dao, type, decorator);
            }

            return decorator;
        }

        protected abstract Decorator<TField, TDAO> Simple(TDAO dao);

        protected virtual Decorator<TField, TDAO> HTML(TDAO dao) => Simple(dao);

        protected virtual Decorator<TField, TDAO> Table(TDAO dao) => Simple(dao);

        protected virtual Decorator<TField, TDAO> FullInfo(TDAO dao) => Card(dao);

        protected virtual Decorator<TField, TDAO> Icon(TDAO dao) => Card(dao);

        protected virtual Decorator<TField, TDAO> Card(TDAO dao) => Table(dao);
    }
}