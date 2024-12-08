﻿using OxDAOEngine.Data;
using OxLibrary;
using OxLibrary.ControlList;

namespace OxDAOEngine.View
{
    public class ItemViewList<TField, TDAO> : List<IItemView<TField, TDAO>>
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new()
    {
        public IItemView<TField, TDAO>? Last =>
            Count > 0
                ? this[Count - 1]
                : null;

        public IItemView<TField, TDAO>? First =>
            Count > 0
                ? this[0]
                : null;

        public OxWidth Bottom
        {
            get
            {
                IItemView<TField, TDAO>? last = Last;
                return last is null
                    ? OxWh.W0
                    : last.Bottom | OxWh.W24;
            }
        }

        public OxPanelList AsPaneList =>
            new OxPanelList().AddRange(this.Select(item => item.AsPane));
    }
}