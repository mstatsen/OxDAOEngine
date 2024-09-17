using System;
using System.Collections.Generic;

namespace OxDAOEngine.Data.Fields
{
    public class FieldsFillingDictionary<T> : Dictionary<FieldsFilling, List<T>>
        where T: Enum
    { };
}