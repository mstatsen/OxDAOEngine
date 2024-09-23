namespace OxDAOEngine.Data.Decorator
{
    public static class DecoratorHelper
    {
        public static string ListToString(List<string?> list, string separator, bool noWrapItems = true)
        {
            string result = string.Empty;
            string itemString;

            for (int i = 0; i < list.Count; i++)
            {
                string? item = list[i];

                if (item == null)
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
            sourceString == null ? string.Empty : sourceString.Replace(" ", "&nbsp;");
    }
}
