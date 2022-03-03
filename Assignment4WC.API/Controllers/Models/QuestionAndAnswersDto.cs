using System.Collections.Generic;

namespace Assignment4WC.API.Controllers.Models
{
    public record QuestionAndAnswersDto(string Question, Dictionary<string,string> Answers);
}