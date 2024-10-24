﻿using OxDAOEngine.Data.Filter.Types;

namespace OxDAOEngine.Data.Filter
{
    public class FilterRules<TField> : ListDAO<FilterRule<TField>>
        where TField : notnull, Enum
    {
        public FilterRule<TField>? this[TField field] =>
            List.Find(rule => rule.Field.Equals(field));

        public FilterRule<TField> Add(TField field) =>
            Add(new FilterRule<TField>(field));

        public FilterRule<TField> Add(TField field, FilterOperation operation) =>
            Add(
                new FilterRule<TField>()
                {
                    Field = field,
                    Operation = operation
                }
            );

        public bool RuleExist(TField field) =>
            this[field] != null;

        public void Remove(TField field)
        {
            FilterRule<TField>? rule = this[field];

            if (rule != null)
                Remove(rule);
        }

        public List<TField> Fields
        {
            get 
            {
                List<TField> fieldList = new();

                foreach (FilterRule<TField> rule in this)
                    fieldList.Add(rule.Field);

                return fieldList;
            }
        }
    }
}