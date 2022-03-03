namespace Assignment4WC.Models.ResultType.LinkReferencerObjects
{
    public interface ILinkReferencerProxy<out TParent>
    {
        TParent AddLink(string content);
        TParent AddLink(string key, string content);
    }
}