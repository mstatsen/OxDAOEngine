using OxXMLEngine.Data.Types;
using OxXMLEngine.XML;
using System.Xml;

namespace OxXMLEngine.Data.Filter
{
    public class SimpleFilter<TField, TDAO> 
        : DAO, IMatcher<TField>, IFieldMapping<TField>
        where TField : notnull, Enum
        where TDAO : DAO, IFieldMapping<TField>, new()
    {
        private FilterConcat concat = FilterConcat.AND;
        private readonly FilterRules<TField> rules = new();
        private readonly Dictionary<TField, object?> calcedValues = new();

        private TDAO itemDAO = new();

        public TDAO ItemDAO
        {
            get => itemDAO;
            set => itemDAO = value;
        }

        public SimpleFilter() { }

        public SimpleFilter(TField field, FilterOperation operation, object value) : this() =>
            AddFilter(field, operation, value);

        public SimpleFilter(TField field, object value) : this() =>
            AddFilter(field, value);

        public SimpleFilter(FilterConcat concat) : this() =>
            this.concat = concat;

        public override void Clear()
        {
            itemDAO.Clear();
            rules.Clear();
            calcedValues.Clear();
        }

        public override void Init() { }

        public object ParseCaldedValue(TField field, string value) => 
            ItemDAO.ParseCaldedValue(field, value);

        protected override void LoadData(XmlElement element)
        {
            itemDAO.Load(element);
            rules.Load(element);
            concat = XmlHelper.Value<FilterConcat>(element, XmlConsts.FilterConcat);
            calcedValues.Clear();

            XmlElement? calcedValuesElement = null;

            foreach (XmlNode node in element.ChildNodes)
                if (node.Name == "CalcedValues")
                {
                    calcedValuesElement = (XmlElement)node;
                    break;
                }

            if (calcedValuesElement != null)
                foreach (XmlNode node in calcedValuesElement.ChildNodes)
                {
                    TField field = XmlHelper.Value<TField>((XmlElement)node, XmlConsts.Field);
                    calcedValues.Add(
                        field,
                        ParseCaldedValue(field, XmlHelper.Value((XmlElement)node, "Value"))
                    );
                }
        }
        protected override void SaveData(XmlElement element, bool clearModified = true)
        {
            itemDAO.Save(element, clearModified);
            rules.Save(element, clearModified);
            XmlHelper.AppendElement(element, XmlConsts.FilterConcat, Concat);

            if (calcedValues.Count == 0)
                return;

            XmlElement calcedValuesElement = XmlHelper.AppendElement(element, "CalcedValues", string.Empty)!;

            foreach (var calcedValue in calcedValues)
            {
                XmlElement calcedValueElement = XmlHelper.AppendElement(calcedValuesElement, "CalcedValue", string.Empty)!;
                XmlHelper.AppendElement(calcedValueElement, XmlConsts.Field, calcedValue.Key);
                XmlHelper.AppendElement(calcedValueElement, "Value", calcedValue.Value);
            }
        }

        public bool FilterIsEmpty =>
            rules.IsEmpty;

        public FilterConcat Concat { get => concat; set => concat = value; }
        public FilterRules<TField> Rules => rules;

        public Dictionary<TField, object?> CalcedValues => calcedValues;

        public object? this[TField field]
        {
            get => GetFieldValue(field);
            set => SetFieldValue(field, value);
        }

        public bool Match(IFieldMapping<TField>? dao)
        {
            if (dao == null)
                return false;

            if (FilterIsEmpty)
                return true;

            MatchAggregator<TField> aggregator = new(concat);

            foreach (FilterRule<TField> rule in rules)
            {
                aggregator.Aggregate(rule.Match(dao, this));

                if (aggregator.Complete)
                    break;
            }

            return aggregator.Matched;
        }

        public SimpleFilter<TField, TDAO> AddFilter(TField field, object? value) =>
            AddFilter(
                field,
                DefaultFilterOperation(field),
                value
            );

        public bool CheckValueFilled(object? value)
        {
            switch(value)
            {
                case NullObject _:
                case null:
                case string stringValue when stringValue == string.Empty:
                    return false;
                default:
                    return true;
            }
        }

        public SimpleFilter<TField, TDAO> AddFilter(TField field, FilterOperation operation, object? value)
        {
            if (operation == FilterOperation.Equals &&
                (value == null || value.ToString() == string.Empty))
                operation = FilterOperation.Blank;

            if (!TypeHelper.Helper<FilterOperationHelper>().IsUnaryOperation(operation))
                if (!CheckValueFilled(value))
                    return this;

            rules.Add(field, operation);

            if (value != null && value is not NullObject)
                this[field] = value;

            return this;
        }

        public object? GetFieldValue(TField field)
        {
            if (IsCalcedField(field))
            {
                calcedValues.TryGetValue(field, out var value);

                if (TypeHelper.FieldIsTypeHelpered(field))
                {
                    object? valueObject = TypeHelper.Value(value);

                    if (valueObject != null)
                        value = valueObject;
                }

                return value;
            }

            return itemDAO[field];
        }

        public void SetFieldValue(TField field, object? value)
        {
            if (IsCalcedField(field))
            {
                if (calcedValues.TryGetValue(field, out var _))
                    calcedValues[field] = value;
                else
                    calcedValues.Add(field, value);
            }
            else
                itemDAO[field] = value;
        }

        public FilterOperation DefaultFilterOperation(TField field) => 
            itemDAO.DefaultFilterOperation(field);

        public bool IsCalcedField(TField field) =>
            itemDAO.IsCalcedField(field);

        public override bool Equals(object? obj) =>
            obj is SimpleFilter<TField, TDAO> otherFilter
            && (base.Equals(obj) || 
                (Concat.Equals(otherFilter.Concat)
                && Rules.Equals(otherFilter.Rules)
                && CalcedValues.Equals(otherFilter.CalcedValues)
                && ItemDAO.Equals(otherFilter.ItemDAO)
            ));

        public override int GetHashCode() =>
            HashCode.Combine(Concat, Rules.GetHashCode(), CalcedValues.GetHashCode(), ItemDAO.GetHashCode());
    }
}