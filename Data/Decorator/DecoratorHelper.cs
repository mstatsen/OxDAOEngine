namespace OxDAOEngine.Data.Decorator
{
    public static class DecoratorHelper
    {
        public static string ListToString<TDAO>(ListDAO<TDAO> list, string separator, bool noWrapItems = true)
            where TDAO : DAO, new() =>
            ListToString(list.List, separator, noWrapItems);

        public static string ListToString<T>(List<T> list, string separator, bool noWrapItems = true)
        { 
            List<string?> stringList = new();

            foreach (T item in list)
                if (item is not null)
                    stringList.Add(item.ToString());

            return ListToString(stringList, separator, noWrapItems);

        }

        public static string ListToString(List<string?> list, string separator, bool noWrapItems = true)
        {
            string result = string.Empty;
            string itemString;

            for (int i = 0; i < list.Count; i++)
            {
                string? item = list[i];

                if (item is null)
                    continue;

                itemString = item;

                if (noWrapItems)
                    itemString = NoWrap(itemString);

                result += itemString;

                if (i < list.Count - 1)
                    result += separator;
            }

            return result;
        }

        public static string NoWrap(string? sourceString) =>
            sourceString is null 
                ? string.Empty 
                : sourceString.Replace(" ", "&nbsp;");
    }
}
