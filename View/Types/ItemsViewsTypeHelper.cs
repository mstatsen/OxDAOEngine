﻿using OxDAOEngine.Data.Types;

namespace OxDAOEngine.View.Types
{
    public class ItemsViewsTypeHelper : AbstractTypeHelper<ItemsViewsType>
    {
        public override ItemsViewsType EmptyValue() => ItemsViewsType.Table;

        public override string GetName(ItemsViewsType value) =>
            value switch
            {
                ItemsViewsType.Table => "Table",
                ItemsViewsType.Cards => "Cards",
                ItemsViewsType.Icons => "Icons",
                ItemsViewsType.Summary => "Summary",
                _ => string.Empty,
            };
    }
}