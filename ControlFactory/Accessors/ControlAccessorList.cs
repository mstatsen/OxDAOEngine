using System.Collections.Generic;

namespace OxXMLEngine.ControlFactory.Accessors
{
    public class ControlAccessorList : List<IControlAccessor>
    {
        public IControlAccessor? First => 
            Count == 0 ? null : this[0];

        public IControlAccessor? Last => 
            Count == 0 ? null : this[Count - 1];
    }
}
