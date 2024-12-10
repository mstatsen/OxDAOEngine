using OxLibrary.ControlList;
using OxLibrary.Geometry;
using OxDAOEngine.Data;

namespace OxDAOEngine.View;

public class ItemViewList<TField, TDAO> :
    List<IItemView<TField, TDAO>>
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

    public short Bottom =>
        OxSH.IfElseZero(Last is not null, Last!.Bottom + 24);

    public OxPanelList AsPaneList =>
        (OxPanelList)new OxPanelList().AddRange(this.Select(item => item.AsPane));
}