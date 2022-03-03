using Microsoft.AspNetCore.Mvc;

namespace Assignment4WC.API.Controllers.Models
{
    public record InitialDetailsDto(string Category, int NumOfQuestions, string Username);
}