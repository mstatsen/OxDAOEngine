namespace OxXMLEngine.Data.Filter
{
    public class FilterRules<TField, TDAO> : ListDAO<FilterRule<TField, TDAO>>
        where TField : notnull, Enum
        where TDAO : DAO, IFieldMapping<TField>
    {
        public FilterRule<TField, TDAO>? this[TField field] =>
            List.Find(rule => rule.Field.Equals(field));

        public FilterRule<TField, TDAO> Add(TField field) =>
            Add(
                new FilterRule<TField, TDAO>()
                { 
                    Field = field
                }
            );

        public FilterRule<TField, TDAO> Add(TField field, FilterOperation operation) =>
            Add(
                new FilterRule<TField, TDAO>()
                {
                    Field = field,
                    Operation = operation
                }
            );

        public bool RuleExist(TField field) =>
            this[field] != null;

        public void Remove(TField field)
        {
            FilterRule<TField, TDAO>? rule = this[field];

            if (rule != null)
                Remove(rule);
        }
    }
}