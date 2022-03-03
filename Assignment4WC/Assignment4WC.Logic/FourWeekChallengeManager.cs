using Assignment4WC.Context;
using Assignment4WC.Context.Models;
using Assignment4WC.Models;
using Assignment4WC.Models.ControllerEndpoints;
using Assignment4WC.Models.ResultType;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Net;

namespace Assignment4WC.Logic
{
    public class FourWeekChallengeManager : IFourWeekChallengeManager
    {
        private readonly AssignmentContext _context;
        private readonly IQuestionRandomiser _questionRandomiser;

        public FourWeekChallengeManager(AssignmentContext context, IQuestionRandomiser questionRandomiser)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _questionRandomiser = questionRandomiser ?? throw new ArgumentNullException(nameof(questionRandomiser));
        }

        public Result AddNewPlayer(int appId, string username, CategoryType category, int numOfQuestions)
        {
            if (DoesUsernameExist(username))
                return new Result(new ErrorMessage(HttpStatusCode.BadRequest, $"The username '{username}' already exists, try another."));

            var result = _questionRandomiser.GetQuestionsWithOrder(numOfQuestions, category);
            if (!result.IsSuccess) return new Result(result.Error);

            _context.Members.Add(new Members
            {
                AppId = appId,
                Username = username,
                QuestionIds = result.Unwrap(),
            });

            _context.SaveChanges();

            return new Result().Ok();
        }

        public Result<Questions> GetCurrentQuestionData(string username)
        {
            var member = GetMemberOrNull(username);
            if (member == null)
                return new Result<Questions>(new ErrorMessage(HttpStatusCode.NotFound,
                    $"Member with username '{username}' does not exist."))
                    .AddLink(FourWeekChallengeEndpoint.StartRoute);

            var currentQuestionIdResult = GetCurrentQuestionId(member);
            if (!currentQuestionIdResult.IsSuccess) 
                return currentQuestionIdResult.ToResult<Questions>();

            var currentQuestionId = currentQuestionIdResult.Unwrap();

            var question = _context.Questions
                .Include(questions => questions.Category)
                .Include(questions => questions.Answers)
                .AsSplitQuery()
                .FirstOrDefault(questions => questions.QuestionId == currentQuestionId);
           
            return question != null ?
                 new Result<Questions>(question) :
                 new Result<Questions>(new ErrorMessage(HttpStatusCode.NotFound, $"Question with ID '{currentQuestionId}' does not exist in database."))
                     .AddLink(FourWeekChallengeEndpoint.InterpolatedGetQuestionRoute(username));
        }

        public Result<bool> SubmitAnswer(string username, string answer)
        {
            var questionDataResult = GetCurrentQuestionData(username);
            
            if (!questionDataResult.IsSuccess)
                return questionDataResult.ToResult<bool>();

            var member = GetMemberOrNull(username)!;
            var questionData = questionDataResult.Unwrap();

            if (questionData.CorrectAnswer == answer)
            {
                if(!string.Equals(answer, "PASS", StringComparison.CurrentCultureIgnoreCase)) AddToUserScore(member);
                member.CurrentQuestionNumber++;
                _context.SaveChanges();
            }

            var result = new Result<bool>(questionData.CorrectAnswer == answer);

            return HasGameEnded(member) ? 
                result.AddLink(FourWeekChallengeEndpoint.InterpolatedGetQuestionRoute(username)):
                result.AddLink(FourWeekChallengeEndpoint.InterpolatedEndGameRoute(username));
        }

        private static bool HasGameEnded(Members member)
        {
            return member.QuestionIds.Split(",").Length == member.CurrentQuestionNumber;
        }

        private Members? GetMemberOrNull(string username) =>
            _context.Members.FirstOrDefault(members => members.Username == username);

        private bool DoesUsernameExist(string username) => 
            _context.Members.Any(members => members.Username == username);

        private static void AddToUserScore(Members member)
        {
            var basePoints = 2;

            if (member.HintAsked)
            {
                basePoints--;
                member.HintAsked = false;
            }

            if (member.LocationHintAsked)
            {
                basePoints--;
                member.LocationHintAsked = false;
            }

            member.UserScore += basePoints;
        }

        private static Result<int> GetCurrentQuestionId(Members member)
        {
            var currentQuestionIndex = member.CurrentQuestionNumber;
            var questionIds = member.QuestionIds.Split(",");

            if (questionIds.Length < currentQuestionIndex)
                return new Result<int>(new ErrorMessage(HttpStatusCode.InternalServerError,
                    $"Index '{nameof(member.CurrentQuestionNumber)}' was outside the range for the number of questionIds the member has."));

            return new Result<int>(int.Parse(questionIds[currentQuestionIndex]));
        }
    }
}
