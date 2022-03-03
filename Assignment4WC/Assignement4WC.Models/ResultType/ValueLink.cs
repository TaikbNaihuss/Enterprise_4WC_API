using System.Collections.Generic;
using System.Net;

namespace Assignment4WC.Models.ResultType
{
    public class ValueLink<T> : IValueLinkDiscriminator<T>
    {
        private readonly HttpStatusCode? _statusCode;
        private readonly bool _isSuccess;
        public T Value { get; }
        public Dictionary<string, string> Links { get; }
        HttpStatusCode? IValueLinkDiscriminator<T>.StatusCode => _statusCode;
        bool IValueLinkDiscriminator<T>.IsSuccess => _isSuccess;

        public ValueLink(T value, Dictionary<string, string> links, bool isSuccess, HttpStatusCode? statusCode)
        {
            Value = value;
            Links = links;
            _statusCode = statusCode;
            _isSuccess = isSuccess;
        }

    }
}