using Assignment4WC.Context.Models;
using Assignment4WC.Models;
using Assignment4WC.Models.ResultType;

namespace Assignment4WC.Logic
{
    public interface IFourWeekChallengeManager
    {
        Result AddNewPlayer(int appId, string username, CategoryType category, int numOfQuestions);
        Result<Questions> GetCurrentQuestionData(string username);
        Result<bool> SubmitAnswer(string username, string answer);
    }
}