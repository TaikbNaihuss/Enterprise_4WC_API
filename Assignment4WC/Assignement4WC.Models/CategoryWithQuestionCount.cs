using System.Collections.Generic;

namespace Assignment4WC.Models
{
    public record CategoryWithQuestionCount(string Category, IEnumerable<int> QuestionCount);
}