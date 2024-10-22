namespace OxDAOEngine.Data.Types
{
    public class IconContentHelper : AbstractTypeHelper<IconContent>
    {
        public override IconContent EmptyValue() => IconContent.Title;

        public override string GetName(IconContent value) =>
            value switch
            {
                IconContent.Image => "Image",
                IconContent.Title => "Title",
                IconContent.Left => "Left",
                IconContent.Right => "Right",
                _ => string.Empty,
            };
    }
}