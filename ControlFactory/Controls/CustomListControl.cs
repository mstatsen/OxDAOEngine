using OxDAOEngine.Data;

namespace OxDAOEngine.ControlFactory.Controls
{
    public abstract class CustomListControl<TField, TDAO, TItems, TItem> 
        : CustomControl<TField, TDAO, TItems>, ICustomListControl<TItem, TItems>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
        where TItems : ListDAO<TItem>, new()
        where TItem : DAO, new()
    {
        protected abstract void ClearValue();

        protected abstract void SetValuePart(TItem valuePart);

        protected abstract void GrabList(TItems list);

        private TItems? fixedItems;

        public TItems? FixedItems 
        { 
            get => fixedItems;
            set
            {
                fixedItems = value;
                OnSetFixedItems();
            } 
        }

        protected virtual void OnSetFixedItems() { }

        protected sealed override TItems GetValue()
        {
            TItems value = new();
            GrabList(value);
            value.Sort();
            return value;
        }

        protected sealed override void SetValue(TItems value)
        {
            ClearValue();

            if (value != null)
            {
                foreach (TItem part in value)
                    SetValuePart(part);

                value.Sort();
            }

            AfterSetValue();
        }

        protected virtual void AfterSetValue() { }

        protected void ResortValue() =>
            SetValue(GetValue());

        protected virtual bool IndentItems => false;

        public override object? PrepareValueToReadOnly(TItems? value)
        {
            string readOnlyValue = string.Empty;

            if (value != null)
            {
                value.Sort();

                string indent = IndentItems ? "        " : string.Empty;
                int maxItemLength = 0;

                if (value.Count > 8)
                    foreach (TItem part in value)
                        maxItemLength = Math.Max(maxItemLength, part.ToString()!.Length);

                string columnDelimiter;

                for (int i = 0; i < 8; i++)
                {
                    if (i == value.Count)
                        break;

                    columnDelimiter = string.Empty;

                    if (value.Count > 8)
                        for (int j = 0; j < 10 + ((maxItemLength - value[i].ToString()!.Length) * 2); j++)
                            columnDelimiter += " ";

                    readOnlyValue += $"{indent}{value[i]}";
                    int i2 = i;

                    while (i2 + 8 < value.Count - 1)
                    {
                        i2 += 8;
                        readOnlyValue += columnDelimiter + value[i2];

                        columnDelimiter = string.Empty;

                        for (int j = 0; j < 10 + ((maxItemLength - value[i2].ToString()!.Length) * 2); j++)
                            columnDelimiter += " ";
                    }

                    readOnlyValue += "\r\n";
                }
            }

            if (readOnlyValue == string.Empty)
                readOnlyValue = "Empty\r\n";

            readOnlyValue = readOnlyValue.Remove(readOnlyValue.Length - 2);
            return readOnlyValue;
        }
    }
}