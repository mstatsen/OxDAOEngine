using OxDAOEngine.Data.Types;
using OxDAOEngine.XML;
using System.Xml;

namespace OxDAOEngine.Data.Filter
{
    public class FilterRule<TField> : DAO
        where TField : notnull, Enum
    {
        public TField Field = default!;
        public FilterOperation Operation;
        public FilterRule() { }

        public FilterRule(TField field) : this() =>
            Field = field;

        public FilterRule(TField field, FilterOperation operation) : this(field) =>
            Operation = operation;

        public override void Clear()
        {
            Field = TypeHelper.DefaultValue<TField>();
            Operation = TypeHelper.DefaultValue<FilterOperation>();
        }

        public override void Init() { }

        protected override void LoadData(XmlElement element)
        {
            Operation = XmlHelper.Value<FilterOperation>(element, XmlConsts.FilterOperation);
            Field = XmlHelper.Value<TField>(element, XmlConsts.Field);
        }

        protected override void SaveData(XmlElement element, bool clearModified = true)
        {
            XmlHelper.AppendElement(element, XmlConsts.FilterOperation, Operation);
            XmlHelper.AppendElement(element, XmlConsts.Field, Field);
        }

        public bool Match(IFieldMapping<TField>? leftObject, IFieldMapping<TField>? rightObject) =>
            TypeHelper.Helper<FilterOperationHelper>()
                .Match(Operation, leftObject?[Field], rightObject?[Field]);

        public override bool Equals(object? obj) =>
            obj is FilterRule<TField> otherRule
            && (base.Equals(obj) ||
                (Field.Equals(otherRule.Field)
                && Operation.Equals(otherRule.Operation))
            );

        public override int GetHashCode() =>
            HashCode.Combine(Field, Operation);
    }
}