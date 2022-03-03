using Assignment4WC.Models;
using Assignment4WC.Models.ResultType;

namespace Assignment4WC.Logic
{
    public interface IQuestionRandomiser
    {
        Result<string> GetQuestionsWithOrder(int numOfQuestions, CategoryType category);
    }
}