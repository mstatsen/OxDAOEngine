using OxLibrary;

namespace OxDAOEngine.Data.Types;

public abstract class AbstractStyledTypeHelper<T>
    : AbstractTypeHelper<T>, IStyledTypeHelper
    where T : Enum
{
    private readonly Dictionary<T, OxColorHelper> colors;

    private OxColorHelper GetTypeColors(T key)
    {
        if (!colors.TryGetValue(key, out var typeColors))
        {
            typeColors = new OxColorHelper(GetBaseColor(key));
            colors.Add(key, typeColors);
        }

        return typeColors;
    }

    public abstract Color GetBaseColor(T value);
    public Color GetBackColor(T value) => GetTypeColors(value).BaseColor;
    public abstract Color GetFontColor(T value);

    public Color BaseColor(object? value) => GetBaseColor((T)Value(value));
    public Color BackColor(object? value) => GetBackColor((T)Value(value));
    public Color FontColor(object? value) => GetFontColor((T)Value(value));

    public AbstractStyledTypeHelper() => 
        colors = new Dictionary<T, OxColorHelper>();
}
