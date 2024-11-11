﻿using OxDAOEngine.Data.Fields;
using OxDAOEngine.Data.Filter.Types;
using OxDAOEngine.Data.Types;
using System.Collections;

namespace OxDAOEngine.Data.Filter
{
    public class FilterGroup<TField, TDAO> : AbstractFilterPart<TField, TDAO>,
        IFieldMapping<TField>,
        IMatcher<TField>, 
        IMatcherList<TField>,
        IEnumerable<FilterRule<TField>>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, IFieldMapping<TField>, new()
    {

        public readonly ListDAO<FilterRule<TField>> Rules = new();

        public FilterGroup() : base() { }

        public FilterGroup(FilterConcat filterConcat) : base(filterConcat) { }

        public FilterRule<TField> Add(TField field, FilterOperation operation, object? value) =>
            Rules.Add(
                new FilterRule<TField>(field, operation, value)
            );

        public FilterRule<TField> Add(TField field, object? value) =>
            Rules.Add(
                new FilterRule<TField>(field, value)
            );

        public FilterRule<TField> Add(FilterRule<TField> rule) =>
            Rules.Add(rule);

        public bool Match(IFieldMapping<TField>? dao) =>
            MatchAggregator<TField>.Match(this, dao);

        public List<IMatcher<TField>> MatchList
        {
            get
            {
                List<IMatcher<TField>> matchList = new();
                matchList.AddRange(Rules);
                return matchList;
            }
        }

        public bool FilterIsEmpty =>
            MatchAggregator<TField>.IsEmpty(this);

        public override void Init()
        {
            base.Init();
            AddMember(Rules);
        }

        public override void Clear()
        {
            base.Clear();
            Rules.Clear();
        }

        public int Count => Rules.Count;

        //TODO:
        public FilterRule<TField> Current =>
            Rules.First;

        public IEnumerator<FilterRule<TField>> GetEnumerator() =>
            Rules.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            Rules.GetEnumerator();

        private readonly FieldHelper<TField> FieldHelper = TypeHelper.FieldHelper<TField>();

        public object ParseCaldedValue(TField field, string value)
        {
            ITypeHelper? helper = FieldHelper.GetHelper(field);
            return helper != null ? helper.Parse(value) : value;
        }

        public FilterOperation DefaultFilterOperation(TField field) => 
            FieldHelper.DefaultFilterOperation(field);

        public bool IsCalcedField(TField field) => 
            FieldHelper.IsCalcedField(field);

        public FilterRule<TField> this[int intex]
        {
            get => Rules[intex];
            set => Rules[intex] = value;
        }

        public List<TField> Fields
        {
            get 
            {
                List<TField> result = new();

                foreach (FilterRule<TField> rule in Rules)
                    result.Add(rule.Field);

                return result;
            }
        }

        public object? this[TField field] 
        {
            get => Rules.Find(r => r.Field.Equals(field))?.Value;
            set
            { 
                FilterRule<TField>? rule = Rules.Find(r => r.Field.Equals(field));

                if (rule != null)
                    rule.Value = value;
            }
        }
    }
}