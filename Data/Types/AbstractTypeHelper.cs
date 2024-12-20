﻿namespace OxDAOEngine.Data.Types
{
    public class TypeObjectList<T> : List<EnumItemObject<T>> where T : Enum { }

    public abstract class AbstractTypeHelper<T> : ITypeHelper, IDependedHelper<T>
        where T : Enum
    {
        private readonly TypeObjectList<T> typeObjects = new();
        public Type ItemType => typeof(T);
        public virtual Type ItemObjectType => typeof(EnumItemObject<T>);

        public virtual bool UseShortNameForControl => false;
        public virtual bool UseFullNameForControl => false;
        public virtual bool UseToolTipForControl => false;

        public abstract string GetName(T value);
        public virtual string GetCaption(T value) => GetName(value);
        public virtual string GetShortName(T value) => value.ToString();
        public virtual string GetFullName(T value) => GetName(value);
        public abstract T EmptyValue();
        public virtual T DefaultValue() => EmptyValue();
        public virtual string GetXmlValue(T value) => GetShortName(value);

        public List<T> All()
        {
            List<T> list = new();

            foreach (T item in Enum.GetValues(typeof(T)))
                list.Add(item);

            return list;
        }

        List<object> ITypeHelper.All()
        {
            List<object> list = new();

            foreach (T item in Enum.GetValues(typeof(T)))
                list.Add(item);

            return list;
        }

        public object Parse(string text)
        {
            foreach (T value in All())
                if (text.Equals(GetXmlValue(value))
                    || text.Equals(GetFullName(value))
                    || text.Equals(GetName(value))
                    || text.Equals(GetShortName(value))
                    || text.Equals(value.ToString()))
                    return value;

            return EmptyValue();
        }

        public EnumItemObject<T>? GetTypeObject(T value)
        {
            EnumItemObject<T>? typeObject = typeObjects.Find(t => t.Value.Equals(value));

            if (typeObject is null)
            {
                typeObject = CreateTypeObject();

                if (typeObject is not null)
                {
                    typeObject.Value = value;
                    typeObjects.Add(typeObject);
                }
            }

            return typeObject;
        }

        protected virtual EnumItemObject<T>? CreateTypeObject() =>
            (EnumItemObject<T>?)Activator.CreateInstance(ItemObjectType);

        private string GetString(object? value, Func<T, string> getter) =>
            value is T tvalue
                ? getter(tvalue)
                : getter(EmptyValue());

        public string Name(object? value) =>
            GetString(value, GetName);

        public string ShortName(object? value) =>
            GetString(value, GetShortName);

        public string FullName(object? value) =>
            GetString(value, GetFullName);

        public string XmlValue(object? value) =>
            GetString(value, GetXmlValue);

        public object Value(object? typeObject) => 
            typeObject switch
            {
                null => (object)EmptyValue,
                T tobject => tobject,
                EnumItemObject<T> uobject => uobject.Value,
                string => Parse(typeObject.ToString()!),
                _ => EmptyValue(),
            };

        public object? TypeObject(object? value) =>
            GetTypeObject((T)Value(value));

        object ITypeHelper.EmptyValue() =>
            EmptyValue();

        object ITypeHelper.DefaultValue() =>
            DefaultValue();

        public List<T> DependedList(object value)
        {
            List<T> list = new();

            foreach (T item in TypeHelper.All<T>())
                if (value.Equals(DependsOnValue(item)))
                    list.Add(item);

            return list;
        }

        public virtual object? DependsOnValue(T? value) =>
            null;

        public U? DependsOnValue<U>(T? value)
            where U : Enum => (U?)DependsOnValue(value);

        public string Caption(object? value) => 
            GetString(value, GetCaption);
    }
}
