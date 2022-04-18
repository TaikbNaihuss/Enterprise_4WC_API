using System.Collections.Generic;
using Assignment4WC.Models.ControllerEndpoints;

namespace Assignment4WC.Models.ResultType.LinkReferencerObjects
{
    //This also purely exists for its visually and structurally nice representation of links stored within.
    public class LinksContainer
    {
        public Dictionary<string, HateoasString> Links { get; set; }

        public static implicit operator Dictionary<string, HateoasString>(LinksContainer link) => link.Links;
        public static explicit operator LinksContainer(Dictionary<string, HateoasString> link) => new() { Links = link };

    }
}
