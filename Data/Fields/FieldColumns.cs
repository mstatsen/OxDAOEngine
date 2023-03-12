using System.Xml;

namespace OxXMLEngine.Data.Fields
{
    public class FieldColumns<TField> : ListDAO<FieldColumn<TField>>
        where TField: Enum
    {
        protected override bool AutoSorting => false;
        public override string DefaultXmlElementName => "Fields";

        protected override void LoadData(XmlElement element)
        {
            base.LoadData(element);

            if (IsEmpty)
                Clear();
        }

        public FieldColumns() : base() { }

        public FieldColumns(List<TField> initialFields) : this() => 
            AddRange(initialFields);

        public FieldColumn<TField> Add(TField field) =>
            Add(new FieldColumn<TField>(field));

        public void AddRange(List<TField>? fields) 
        {
            if (fields != null)
                foreach (TField field in fields)
                    Add(field);
        }

        public List<TField> Fields
        {
            get 
            {
                List<TField> result = new();

                foreach (FieldColumn<TField> item in List)
                    result.Add(item.Field);

                return result;
            }
        }
    }
}