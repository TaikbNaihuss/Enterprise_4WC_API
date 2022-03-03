using System;
using System.Collections.Generic;
using System.Net;
using Assignment4WC.Models.ResultType.LinkReferencerObjects;

namespace Assignment4WC.Models.ResultType
{
    public class ResultErrorMessage : ErrorMessage, ILinkReferencerProxy<ResultErrorMessage>
    {

        private readonly ILinkReferencer _linkRef;

        public ResultErrorMessage(ErrorMessage error) : base(error.StatusCode, error.Message)
        {
            _linkRef = new LinkReferencer();
        }


        public ResultErrorMessage AddLink(string content)
        {
            _linkRef.AddLink(content);
            return this;
        }

        public ResultErrorMessage AddLink(string key, string content)
        {
            _linkRef.AddLink(key, content);
            return this;
        }

        public Dictionary<string, string> GetLinks() => _linkRef.GetLinks();
    }
}