using Assignment4WC.Models.ResultType;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Assignment4WC.API.Extensions
{
    public static class ResultExtensions
    {
        public static IActionResult ToActionResult<T>(this IValueLinkDiscriminator<T> valueLink, ControllerBase controllerBase)
        {
            return valueLink.IsSuccess ?
                MapToSuccessStatusCode(valueLink, controllerBase):
                MapToErrorStatusCode(valueLink, controllerBase);
        }

        private static IActionResult MapToSuccessStatusCode<T>(IValueLinkDiscriminator<T> valueLink, ControllerBase controllerBase)
        {
            return valueLink.StatusCode switch
            {
                HttpStatusCode.OK => controllerBase.Ok((ValueLink<T>)valueLink),
                _ => null
            };
        }

        private static IActionResult MapToErrorStatusCode<T>(IValueLinkDiscriminator<T> valueLink, ControllerBase controllerBase)
        {
            return valueLink.StatusCode switch
            {
                HttpStatusCode.BadRequest => controllerBase.BadRequest((ValueLink<T>)valueLink),
                HttpStatusCode.NotFound => controllerBase.NotFound((ValueLink<T>)valueLink),
                HttpStatusCode.InternalServerError => controllerBase.StatusCode(StatusCodes.Status500InternalServerError, (ValueLink<T>)valueLink),
                _ => null
            };
        }
    }
}