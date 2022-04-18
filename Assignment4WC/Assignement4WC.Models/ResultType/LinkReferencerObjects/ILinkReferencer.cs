using Assignment4WC.Models.ControllerEndpoints;

namespace Assignment4WC.Models.ResultType.LinkReferencerObjects
{
    public interface ILinkReferencer
    {
        LinkReferencer AddLink(HateoasString content);
        LinkReferencer AddLink(string key, HateoasString content);
        LinksContainer GetLinks();
    }
}