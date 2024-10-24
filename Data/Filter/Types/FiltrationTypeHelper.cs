﻿using OxDAOEngine.Data.Types;

namespace OxDAOEngine.Data.Filter.Types
{
    public class FiltrationTypeHelper : AbstractTypeHelper<FiltrationType>
    {
        public override FiltrationType EmptyValue() =>
            FiltrationType.StandAlone;

        public override string GetName(FiltrationType value) =>
            value switch
            {
                FiltrationType.StandAlone => "StandAlone",
                FiltrationType.IncludeParent => "IncludeParent",
                FiltrationType.BaseOnChilds => "BaseOnChilds",
                _ => string.Empty,
            };
    }
}