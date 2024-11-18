using OxDAOEngine.Data.Fields;

namespace OxDAOEngine.Data.Types
{
    public static class TypeHelper
    {
        public static THelper Register<THelper>()
            where THelper : ITypeHelper, new()
        {
            foreach (ITypeHelper existHelper in helperList)
                if (existHelper is THelper tHelper)
                    return tHelper;

            THelper helper = new();
            helperList.Add(helper);
            return helper;
        }

        static TypeHelper()
        {
            anyObject = new NullObject("Any");
            helperList = new List<ITypeHelper>();
        }

        private static readonly NullObject anyObject;
        private static readonly List<ITypeHelper> helperList;

        public static NullObject AnyObject => anyObject;

        public static ITypeHelper Helper(object value)
        {
            foreach (ITypeHelper helper in helperList)
                if (value.GetType() == helper.ItemType
                    || value.GetType() == helper.ItemObjectType)
                    return helper;

            throw new NotSupportedException();
        }

        public static List<TItem> All<TItem>() where TItem : Enum
        {
            List<TItem> list = new();

            foreach (TItem item in Enum.GetValues(typeof(TItem)))
                list.Add(item);

            return list;
        }

        public static List<TItem> Actual<TItem>() where TItem : Enum
        {
            List<TItem> list = new();

            foreach (TItem item in All<TItem>())
                if (!item.Equals(EmptyValue<TItem>()))
                    list.Add(item);

            return list;
        }

        public static THelper Helper<THelper>()
            where THelper : ITypeHelper
        {
            foreach (ITypeHelper helper in helperList)
                if (helper is THelper tHelper)
                    return tHelper;

            throw new NotSupportedException();
        }

        public static ITypeHelper HelperByItemType<TItem>()
            where TItem : notnull, Enum =>
            Helper(Enum.GetValues(typeof(TItem)).GetValue(0)!);

        public static FieldHelper<TField> FieldHelper<TField>()
            where TField : notnull, Enum =>
            (FieldHelper<TField>)Helper(default(TField)!)!;

        public static FieldGroupHelper<TField, TFieldGroup> FieldGroupHelper<TField, TFieldGroup>()
            where TField : notnull, Enum
            where TFieldGroup : notnull, Enum => 
            (FieldGroupHelper<TField, TFieldGroup>)Helper(default(TFieldGroup)!);

        public static IStyledTypeHelper StyledHelper(object value) =>
            Helper(value) is IStyledTypeHelper styledHelper 
                ? styledHelper 
                : throw new NotSupportedException();

        public static string Name(object? value) => 
            value is null 
                ? string.Empty 
                : IsTypeHelpered(value)
                    ? Helper(value).Name(value) 
                    : value.ToString()!;

        public static string ShortName(object? value) =>
            value is null
                ? string.Empty
                : IsTypeHelpered(value)
                    ? Helper(value).ShortName(value)
                    : value.ToString()!;
        public static string FullName(object? value) =>
            value is null
                ? string.Empty
                : IsTypeHelpered(value)
                    ? Helper(value).FullName(value)
                    : value.ToString()!;

        public static string XmlValue(object? value) =>
            value is null
                ? string.Empty
                : IsTypeHelpered(value)
                    ? Helper(value).XmlValue(value)
                    : value.ToString()!;

        public static object? Value(object? typeObject) =>
            typeObject is null
                ? null
                : IsTypeHelpered(typeObject)
                    ? Helper(typeObject).Value(typeObject)
                    : typeObject;

        public static TItem Value<TItem>(object? typeObject) 
            where TItem : notnull, Enum
        {
            object? value = Value(typeObject);
            return 
                value is not null 
                    ? (TItem)value 
                    : default!;
        }

        public static EnumItemObject<TItem>? TypeObject<TItem>(object value) 
            where TItem : Enum =>
            (EnumItemObject<TItem>?)TypeObject(value);

        public static object? TypeObject(object? value) =>
            value is null 
                ? AnyObject
                : Helper(value).TypeObject(value);

        public static TItem EmptyValue<TItem>() 
            where TItem : notnull, Enum =>
            (TItem)Helper(All<TItem>()[0]).EmptyValue();

        public static int ItemsCount<TItem>() where TItem : notnull, Enum =>
            All<TItem>().Count;

        public static TItem DefaultValue<TItem>() where TItem : notnull, Enum =>
            (TItem)Helper(All<TItem>()[0]).DefaultValue();

        public static TItem Parse<TItem>(string text) where TItem : notnull, Enum =>
            (TItem)Helper(All<TItem>()[0]).Parse(text);

        public static Color BaseColor(object? value) =>
            value is null 
                ? Color.Honeydew 
                : StyledHelper(value).BaseColor(value);

        public static Color BackColor(object? value) =>
            value is null
                ? Color.Honeydew
                : StyledHelper(value).BackColor(value);

        public static Color FontColor(object? value) =>
            value is null
                ? Color.Black
                : StyledHelper(value).FontColor(value);

        public static bool IsTypeHelpered<TItem>()
            where TItem : notnull, Enum =>
            helperList.Find(H =>
                (H.ItemType == typeof(TItem)) ||
                (H.ItemObjectType == typeof(TItem))
            ) is not null;

        public static bool IsTypeHelpered(object? value) =>
            value is not null 
            && helperList.Find(h =>
                    h.ItemType.Equals(value.GetType()) 
                    || h.ItemObjectType.Equals(value.GetType())
                ) is not null;

        public static bool FieldIsTypeHelpered<TField>(TField field)
            where TField : notnull, Enum =>
            FieldHelper<TField>().GetHelper(field) is not null;

        public static List<TDepended> DependedList<TDepended>(object value)
            where TDepended : notnull, Enum
        {
            if (Helper(EmptyValue<TDepended>()) is IDependedHelper<TDepended> dependedHelper)
                return dependedHelper.DependedList(value);

            return All<TDepended>();
        }

        public static List<TDepended> DependedList<TField, TDepended>(TField field, object value)
            where TField : notnull, Enum
            where TDepended : notnull, Enum
        {
            if (Helper(EmptyValue<TDepended>()) is IDependedHelper<TField, TDepended> dependedHelper)
                return dependedHelper.DependedList(field, value);

            return DependedList<TDepended>(value);
        }

        public static object? DependsOnValue<TDepended>(TDepended value)
            where TDepended : notnull, Enum
        {
            if (Helper(EmptyValue<TDepended>()) is IDependedHelper<TDepended> dependedHelper)
                return dependedHelper.DependsOnValue(value);

            return null;
        }

        public static TDependsOn? DependsOnValue<TDepended, TDependsOn>(TDepended value)
            where TDepended : notnull, Enum
            where TDependsOn : Enum
        {
            ITypeHelper helper = Helper(EmptyValue<TDepended>());

            if (helper is IDependedHelper<TDepended> dependedHelper)
                return dependedHelper.DependsOnValue<TDependsOn>(value);

            return default;
        }

        public static string Caption(object? value) =>
            value is null
                ? string.Empty
                : IsTypeHelpered(value)
                    ? Helper(value).Caption(value)
                    : value.ToString()!;
    }
}