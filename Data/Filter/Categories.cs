using OxDAOEngine.Data.Filter.Types;
using OxDAOEngine.Data.Types;

namespace OxDAOEngine.Data.Filter
{
    public class Categories<TField, TDAO> : TreeDAO<Category<TField, TDAO>>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public override string DefaultXmlElementName => "Categories";

        public Category<TField, TDAO> Add(TField field) => 
            Add(
                new Category<TField, TDAO>("By " + TypeHelper.FieldHelper<TField>().Name(field))
                {
                    Type = CategoryType.FieldExtraction,
                    Field = field
                }
            );
    }
}