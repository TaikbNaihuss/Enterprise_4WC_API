using System.Net;

namespace Assignment4WC.Models.ResultType
{
    public interface IValueLinkDiscriminator<out T>
    {
        bool IsSuccess { get; }
        HttpStatusCode? StatusCode { get; }
    }
}