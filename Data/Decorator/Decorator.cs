namespace OxDAOEngine.Data.Decorator
{
    public abstract class Decorator<TField, TDAO>
        where TField: Enum
        where TDAO : RootDAO<TField>, new()
    {
        public Decorator(TDAO dao) =>
            Dao = dao;

        public object? this[TField field] =>
            Value(field);

        public abstract object? Value(TField field);

        public virtual string Attributes(TField field) =>
            string.Empty;

        public TDAO Dao { get; set; }

        public static object NormalizeIfEmpty(object? value) => 
            (value == null) || 
            (value.ToString() == null) || 
            (value.ToString()?.Trim() == string.Empty) ? Consts.No : value;

        protected Decorator<TField, TDAO> OtherDecorator(DecoratorType type) =>
            DataManager.DecoratorFactory<TField, TDAO>().Decorator(type, Dao);
    }
}
