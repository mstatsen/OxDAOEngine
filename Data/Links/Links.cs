namespace OxDAOEngine.Data.Links
{
    public class Links<TField> : ListDAO<Link<TField>>
        where TField : notnull, Enum
    {
        public Links() { }

        public override string DefaultXmlElementName => "Links";
    }
}
