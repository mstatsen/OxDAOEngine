namespace OxDAOEngine.Data.Types
{
    public abstract class FieldAccordingHelper<TField, TDepended> : AbstractStyledTypeHelper<TDepended>,
        IDependedHelper<TField, TDepended>
        where TField : notnull, Enum
        where TDepended : notnull, Enum
    {
        public virtual List<TDepended> DependedList(TField field, object value) => All();

        public override Color GetBaseColor(TDepended value) => default;

        public override Color GetFontColor(TDepended value) => default;
    }
}