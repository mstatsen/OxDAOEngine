namespace OxDAOEngine.Data.Types
{
    public class NullObject : IEmptyChecked
    {
        private readonly string Text;

        public NullObject(string text) =>
            Text = text;

        public bool IsEmpty => true;

        public override string ToString() =>
            Text;
    }
}