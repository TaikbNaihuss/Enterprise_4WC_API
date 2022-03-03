using System.Collections.Generic;

namespace Assignment4WC.Models.ResultType.LinkReferencerObjects
{
    public class LinksContainer
    {
        public Dictionary<string, string> Links { get; set; }

        public static implicit operator Dictionary<string, string>(LinksContainer link) => link.Links;
        public static explicit operator LinksContainer(Dictionary<string, string> link) => new() { Links = link };

    }
}
