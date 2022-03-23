using System.Collections.Generic;

namespace Assignment4WC.Models.ResultType.LinkReferencerObjects
{
    //This also purely exists for its visually and structurally nice representation of links stored within.
    public class LinksContainer
    {
        public Dictionary<string, string> Links { get; set; }

        public static implicit operator Dictionary<string, string>(LinksContainer link) => link.Links;
        public static explicit operator LinksContainer(Dictionary<string, string> link) => new() { Links = link };

    }
}
