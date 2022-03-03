namespace Assignment4WC.Models.ResultType.LinkReferencerObjects
{
    public interface ILinkReferencer
    {
        LinkReferencer AddLink(string content);
        LinkReferencer AddLink(string key, string content);
        LinksContainer GetLinks();
    }
}