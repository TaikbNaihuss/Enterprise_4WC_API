using System.Collections.Generic;

namespace Assignment4WC.Models.ResultType.LinkReferencerObjects
{
    public class LinkReferencer : ILinkReferencer
    {
        protected readonly Dictionary<string, string> _links;

        public LinkReferencer()
        {
            _links = new Dictionary<string, string>();
        }

        public LinkReferencer AddLink(string content)
        {
            _links.Add("href", content);
            return this;
        }

        public LinkReferencer AddLink(string key, string content)
        {
            _links.Add(key, content);
            return this;
        }

        public LinksContainer GetLinks() => (LinksContainer)_links;
    }
}