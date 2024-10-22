using OxDAOEngine.Data.Fields.Types;

namespace OxDAOEngine.Data.Fields
{
    public class FieldsFillingDictionary<T> : Dictionary<FieldsFilling, List<T>>
        where T: Enum
    { };
}