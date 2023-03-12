using OxXMLEngine.Data;
using System;

namespace OxXMLEngine.ControlFactory.Controls
{
    public abstract class CustomListControl<TField, TDAO, TItems, TItem> 
        : CustomControl<TField, TDAO,TItems>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
        where TItems : ListDAO<TItem>, new()
        where TItem : DAO, new()
    {
        protected abstract void ClearValue();

        protected abstract void SetValuePart(TItem valuePart);

        protected abstract void GrabList(TItems list);

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
    }
}