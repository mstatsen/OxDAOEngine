namespace OxDAOEngine.Data.Types
{
    public interface ILinkHelper<TField> : ITypeHelper, IStyledTypeHelper
    {
        bool IsMandatoryLink(object value);
        TField ExtractFieldName { get; }
    }
}
