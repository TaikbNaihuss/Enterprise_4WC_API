using Assignment4WC.Models.ResultType.LinkReferencerObjects;
using System.Collections.Generic;
using Assignment4WC.Models.ControllerEndpoints;

namespace Assignment4WC.Models.ResultType
{
    public class ResultErrorMessage : ErrorMessage, ILinkReferencerProxy<ResultErrorMessage>
    {

        private readonly ILinkReferencer _linkRef;

        public ResultErrorMessage(ErrorMessage error) : base(error.StatusCode, error.Message)
        {
            _linkRef = new LinkReferencer();
        }

        public ResultErrorMessage AddLink(HateoasString content)
        {
            _linkRef.AddLink(content);
            return this;
        }

        public ResultErrorMessage AddLink(string key, HateoasString content)
        {
            _linkRef.AddLink(key, content);
            return this;
        }

        public Dictionary<string, HateoasString> GetLinks() => _linkRef.GetLinks();
    }
}