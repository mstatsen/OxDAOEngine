using System;
using System.Collections.Generic;

namespace OxXMLEngine.Data.Fields
{
    public class FieldDictionary<T, U> : Dictionary<T, U> 
        where T: Enum
    { }
}