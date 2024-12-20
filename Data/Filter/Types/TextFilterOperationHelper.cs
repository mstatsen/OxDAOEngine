﻿using OxDAOEngine.Data.Types;

namespace OxDAOEngine.Data.Filter.Types
{
    public class TextFilterOperationHelper : AbstractTypeHelper<TextFilterOperation>
    {
        public override TextFilterOperation EmptyValue() =>
            TextFilterOperation.Contains;

        public override string GetName(TextFilterOperation value) =>
            value switch
            {
                TextFilterOperation.Contains => "Contains",
                TextFilterOperation.StartsWith => "StartsWith",
                TextFilterOperation.EndsWith => "EndsWith",
                _ => string.Empty,
            };

        public FilterOperation Operation(TextFilterOperation value) =>
            value switch
            {
                TextFilterOperation.StartsWith => FilterOperation.StartsWith,
                TextFilterOperation.EndsWith => FilterOperation.EndsWith,
                _ => FilterOperation.Contains,
            };

        public string DisplaySQLText(TextFilterOperation operation, object? value) =>
            operation switch
            {
                TextFilterOperation.StartsWith => $"{value}%",
                TextFilterOperation.EndsWith => $"%{value}",
                _ => $"%{value}%",
            };
    }
}