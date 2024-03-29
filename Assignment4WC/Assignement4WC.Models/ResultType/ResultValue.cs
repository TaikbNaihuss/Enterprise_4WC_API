﻿#nullable enable
using System;
using System.Collections.Generic;
using System.Net;
using Assignment4WC.Models.ControllerEndpoints;
using Assignment4WC.Models.ResultType.LinkReferencerObjects;

namespace Assignment4WC.Models.ResultType
{
    public class ResultValue<T> : ILinkReferencerProxy<ResultValue<T>>
    {
        public T Value { get; }
        public HttpStatusCode StatusCode { get; }

        private readonly ILinkReferencer _linkRef;

        public ResultValue(T value)
        {
            Value = value;
            StatusCode = HttpStatusCode.OK;
            _linkRef = new LinkReferencer();
        }

        public ResultValue<T> AddLink(HateoasString content)
        {
            _linkRef.AddLink(content);
            return this;
        }

        public ResultValue<T> AddLink(string key, HateoasString content)
        {
             _linkRef.AddLink(key, content);
             return this;
        }

        public Dictionary<string, HateoasString> GetLinks() => _linkRef.GetLinks();

        public static implicit operator T(ResultValue<T> resultVal) => resultVal.Value;
        public static explicit operator ResultValue<T>(T val) => new(val!);
    }
}