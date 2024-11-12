using OxDAOEngine.Data.Fields;
using OxDAOEngine.Data.Filter.Types;
using OxDAOEngine.Data.Types;
using OxDAOEngine.XML;
using System.Xml;

namespace OxDAOEngine.Data.Filter
{
    public class FilterRule<TField> : DAO,
        IFieldMapping<TField>,
        IMatcher<TField>
        where TField : notnull, Enum
    {
        public TField Field = default!;
        public FilterOperation Operation;
        public object? Value;

        public FilterRule() { }

        public FilterRule(TField field) : this() =>
            Field = field;

        public FilterRule(TField field, FilterOperation operation) : this(field) =>
            Operation = operation;

        public FilterRule(TField field, FilterOperation operation, object? value) : this(field, operation) =>
            Value = value;

        public FilterRule(TField field, object? value) : this(field)
        {
            Operation = DefaultFilterOperation(field);
            Value = value;
        }

        public override void Clear()
        {
            Field = TypeHelper.DefaultValue<TField>();
            Operation = TypeHelper.DefaultValue<FilterOperation>();
            Value = null;
        }

        public override void Init() { }

        protected override void LoadData(XmlElement element)
        {
            Operation = XmlHelper.Value<FilterOperation>(element, XmlConsts.Operation);
            Field = XmlHelper.Value<TField>(element, XmlConsts.Field);
            Value = XmlHelper.Value(element, XmlConsts.Value);
        }

        protected override void SaveData(XmlElement element, bool clearModified = true)
        {
            XmlHelper.AppendElement(element, XmlConsts.Operation, Operation);
            XmlHelper.AppendElement(element, XmlConsts.Field, Field);
            XmlHelper.AppendElement(element, XmlConsts.Value, Value);
        }

        public bool Match(IFieldMapping<TField>? leftObject) =>
            TypeHelper.Helper<FilterOperationHelper>().
                Match(Operation, leftObject?[Field], TypeHelper.Value(Value));

        public override bool Equals(object? obj) =>
            obj is FilterRule<TField> otherRule
            && (base.Equals(obj) ||
                (Field.Equals(otherRule.Field)
                && Operation.Equals(otherRule.Operation))
            );

        public override int GetHashCode() =>
            HashCode.Combine(Field, Operation);

        public object? this[TField field] 
        { 
            get => Value;
            set => Value = value; 
        }

        private readonly FieldHelper<TField> FieldHelper = TypeHelper.FieldHelper<TField>();

        public bool FilterIsEmpty => false;

        public object ParseCaldedValue(TField field, string value)
        {
            ITypeHelper? helper = FieldHelper.GetHelper(field);
            return helper != null 
                ? helper.Parse(value) 
                : value;
        }

        public FilterOperation DefaultFilterOperation(TField field) => 
            FieldHelper.DefaultFilterOperation(field);

        public bool IsCalcedField(TField field) => 
            FieldHelper.IsCalcedField(field);
    }
}