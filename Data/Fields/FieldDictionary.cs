using System;
using System.Collections.Generic;

namespace OxDAOEngine.Data.Fields
{
    public class FieldDictionary<T, U> : Dictionary<T, U> 
        where T: Enum
    { }
}