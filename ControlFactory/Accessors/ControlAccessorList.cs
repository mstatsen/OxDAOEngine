namespace OxDAOEngine.ControlFactory.Accessors
{
    public class ControlAccessorList : List<IControlAccessor>
    {
        public IControlAccessor? First => 
            Count is 0 
                ? null 
                : this[0];

        public IControlAccessor? Last => 
            Count is 0 
                ? null 
                : this[Count - 1];
    }
}
