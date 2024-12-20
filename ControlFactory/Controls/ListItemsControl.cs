﻿using OxDAOEngine.Data;
using OxLibrary.Controls;

namespace OxDAOEngine.ControlFactory.Controls
{
    public abstract class ListItemsControl<TList, TItem, TEditor, TField, TDAO> 
        : CustomItemsContainerControl<TList, TItem, OxListBox, TEditor, TField, TDAO>
        where TList : ListDAO<TItem>, new ()
        where TItem : DAO, new ()
        where TEditor : CustomItemEditor<TItem, TField, TDAO>, new ()
        where TField : notnull, Enum
        where TDAO : RootDAO<TField>, new ()
    {
    }
}
