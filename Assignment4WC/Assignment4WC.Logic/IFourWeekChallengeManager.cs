using System.Collections.Generic;
using Assignment4WC.Context.Models;
using Assignment4WC.Models;
using Assignment4WC.Models.ResultType;

namespace Assignment4WC.Logic
{
    public interface IFourWeekChallengeManager
    {
        Result AddNewPlayer(int appId, string username, CategoryType category, int numOfQuestions);
        Result<Questions> GetCurrentQuestionData(string username) ;
        Result<bool> SubmitAnswer(string username, string answer);
        Result<List<CategoryWithQuestionCount>> GetCategoriesAndQuestionCount();
        Result UpdateUserLocation(string username, decimal latitude, decimal longitude);
        Result<string> GetHintFromQuestion(string username);
        Result<string> GetLocationHintFromQuestion(string username);
        Result EndGame(string username);
        Result<int> GetUserScore(string username);
        Result<List<UserScore>> GetHighScores();
    }
}