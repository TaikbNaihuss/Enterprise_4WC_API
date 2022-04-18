using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assignment4WC.Models.ControllerEndpoints
{
    //An object composed of a RouteName e.g "categories" with the appropriate HTTP Verb/HTTP Action attached E.g "GET
    public record HateoasString(string RouteName, string HttpAction);
}
