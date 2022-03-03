using System.Net;

namespace Assignment4WC.Models
{
    public class ErrorMessage
    {
        public HttpStatusCode? StatusCode { get; }
        public string Message { get; set; }

        public ErrorMessage(HttpStatusCode? statusCode, string message)
        {
            StatusCode = statusCode;
            Message = message;
        }

    }
}