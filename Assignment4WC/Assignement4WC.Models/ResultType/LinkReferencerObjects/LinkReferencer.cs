using System.Collections.Generic;
using Assignment4WC.Models.ControllerEndpoints;

namespace Assignment4WC.Models.ResultType.LinkReferencerObjects
{
    public class LinkReferencer : ILinkReferencer
    {
        protected readonly Dictionary<string, HateoasString> _links;

        public LinkReferencer()
        {
            _links = new Dictionary<string, HateoasString>();
        }

        public LinkReferencer AddLink(HateoasString content)
        {
            _links.Add("href", content);
            return this;
        }

        public LinkReferencer AddLink(string key, HateoasString content)
        {
            _links.Add(key, content);
            return this;
        }

        public LinksContainer GetLinks() => (LinksContainer)_links;
    }
}