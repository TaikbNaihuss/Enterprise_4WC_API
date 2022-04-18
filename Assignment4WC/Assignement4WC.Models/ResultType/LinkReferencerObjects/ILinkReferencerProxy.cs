using Assignment4WC.Models.ControllerEndpoints;

namespace Assignment4WC.Models.ResultType.LinkReferencerObjects
{
    public interface ILinkReferencerProxy<out TParent>
    {
        TParent AddLink(HateoasString content);
        TParent AddLink(string key, HateoasString content);
    }
}