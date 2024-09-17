namespace OxDAOEngine.Data.Types
{
    public class NullObject
    {
        private readonly string Text;

        public NullObject(string text) =>
            Text = text;

        public override string ToString() =>
            Text;
    }
}