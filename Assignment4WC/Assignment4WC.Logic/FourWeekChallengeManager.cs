#nullable enable
using Assignment4WC.Context;
using Assignment4WC.Context.Models;
using Assignment4WC.Models;
using Assignment4WC.Models.ControllerEndpoints;
using Assignment4WC.Models.ResultType;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;

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

        public Result<List<CategoryWithQuestionCount>> GetCategoriesAndQuestionCount() =>
            new Result<List<CategoryWithQuestionCount>>(
                    Enum.GetNames(typeof(CategoryType))
                        .Select(categoryString => (CategoryType)Enum.Parse(typeof(CategoryType), categoryString))
                        .Select(category => new CategoryWithQuestionCount(
                            category.ToString(),
                            _context.Questions.Include(questions => questions.Category)
                                .AsSplitQuery()
                                .Count(questions => questions.Category.CategoryName == category)))
                        .ToList())
                .AddLink(FourWeekChallengeEndpoint.StartRoute);

        public Result UpdateUserLocation(string username, decimal latitude, decimal longitude)
        {
            var member = GetMemberOrNull(username);
            if (member == null)
                return GetMemberDoesNotExistError(username);

            var newLocation = new Locations()
            {
                Latitude = latitude,
                Longitude = longitude
            };

            if (member.LocationId == 0)
            {
                _context.Locations.Add(newLocation);
                member.LocationId = _context.Locations.First(locations => locations == newLocation).LocationId;
            }
            else
            {
                var userLocation = _context.Locations.First(locations => locations.LocationId == member.LocationId);
                userLocation.Latitude = latitude;
                userLocation.Longitude = longitude;
            }

            _context.SaveChanges();

            return new Result(null);
        }

        public Result<string> GetHintFromQuestion(string username)
        {
            var questionDataResult = GetCurrentComplexQuestionData(username);
            if (!questionDataResult.IsSuccess)
                return questionDataResult.ToResult<string>();

            return questionDataResult.HasValue()
                ? new Result<string>(questionDataResult.Unwrap().Hint)
                : new Result<string>(string.Empty);
        }

        public Result AddNewPlayer(int appId, string username, CategoryType category, int numOfQuestions)
        {
            if (DoesUsernameExist(username))
                return new Result(new ErrorMessage(HttpStatusCode.BadRequest,
                    $"The username '{username}' already exists, try another."));

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
                return GetMemberDoesNotExistError<Questions>(username);

            var currentQuestionIdResult = GetCurrentQuestionId(member);
            if (!currentQuestionIdResult.IsSuccess)
                return currentQuestionIdResult.ToResult<Questions>();

            var currentQuestionId = currentQuestionIdResult.Unwrap();

            var question = GetQuestion(currentQuestionId);
            if (question == null)
                return new Result<Questions>(
                        new ErrorMessage(
                            HttpStatusCode.NotFound,
                            $"Question with ID '{currentQuestionId}' does not exist in database."))
                    .AddLink(FourWeekChallengeEndpoint.GetQuestionRouteWith(username));
            
            var result = question.Discriminator == QuestionComplexity.Simple.ToString()
                    ? new Result<Questions>(question)
                    : new Result<Questions>(question)
                        .AddLink("hint", FourWeekChallengeEndpoint.GetHintRouteWith(username))
                        .AddLink("locationHint", FourWeekChallengeEndpoint.GetLocationHintRouteWith(username));

            return result;
        }


        public Result<string> GetLocationHintFromQuestion(string username)
        {
            var questionDataResult = GetCurrentComplexQuestionData(username);
            if (!questionDataResult.IsSuccess)
                return questionDataResult.ToResult<string>();

            return questionDataResult.HasValue()
                ? new Result<string>(questionDataResult.Unwrap().LocationHint)
                : new Result<string>(string.Empty);
        }

        public Result EndGame(string username)
        {
            var member = GetMemberOrNull(username);
            if (member == null)
                return GetMemberDoesNotExistError(username);

            return HasGameEnded(member)
                ? new Result().Ok()
                : new Result(new ErrorMessage(HttpStatusCode.BadRequest,
                    $"Game has not ended for user with name '{username}.'"));

        }

        public Result<int> GetUserScore(string username)
        {
            var member = GetMemberOrNull(username);
            return member != null ? 
                new Result<int>(member.UserScore) 
                : GetMemberDoesNotExistError<int>(username);
        }

        public Result<List<UserScore>> GetHighScores()
        {
            if (!_context.Members.Any())
                return new Result<List<UserScore>>(
                        new ErrorMessage(HttpStatusCode.BadRequest, "No members currently exist."))
                    .AddLink("initialise", FourWeekChallengeEndpoint.InitialiseGameRoute);
            
            return new Result<List<UserScore>>(_context.Members.OrderBy(members => members.UserScore)
                .Select(members => new UserScore(members.Username, members.UserScore))
                .ToList())
                .AddLink("initialise", FourWeekChallengeEndpoint.InitialiseGameRoute);
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
                if (!string.Equals(answer, "PASS", StringComparison.CurrentCultureIgnoreCase)) 
                    AddToUserScore(member);
                member.CurrentQuestionNumber++;
                _context.SaveChanges();
            }

            var result = new Result<bool>(questionData.CorrectAnswer == answer);

            return HasGameEnded(member) ?
                result.AddLink(FourWeekChallengeEndpoint.GetQuestionRouteWith(username)) :
                result.AddLink(FourWeekChallengeEndpoint.EndGameRouteWith(username));
        }

        private Result<ComplexQuestions> GetCurrentComplexQuestionData(string username)
        {
            var member = GetMemberOrNull(username);
            if (member == null)
                return GetMemberDoesNotExistError<ComplexQuestions>(username);

            var currentQuestionIdResult = GetCurrentQuestionId(member);
            if (!currentQuestionIdResult.IsSuccess)
                return currentQuestionIdResult.ToResult<ComplexQuestions>();

            var currentQuestionId = currentQuestionIdResult.Unwrap();

            var complexQuestion = GetComplexQuestion(currentQuestionId);

            var questionExists = _context.Questions.Any(questions => questions.QuestionId == currentQuestionId);

            return complexQuestion != null ?
                new Result<ComplexQuestions>(complexQuestion) :
                questionExists ?
                    new Result<ComplexQuestions>().Ok() :
                    new Result<ComplexQuestions>(new ErrorMessage(HttpStatusCode.NotFound, $"Question with ID '{currentQuestionId}' does not exist in database."))
                        .AddLink(FourWeekChallengeEndpoint.GetQuestionRouteWith(username));
        }

        private Questions? GetQuestion(int currentQuestionId)
        {
            return _context.Questions
                .Include(questions => questions.Category)
                .Include(questions => questions.Answers)
                .AsSplitQuery()
                .FirstOrDefault(questions => questions.QuestionId == currentQuestionId);
        }

        private ComplexQuestions? GetComplexQuestion(int currentQuestionId)
        {
            return _context.ComplexQuestions
                .Include(questions => questions.Category)
                .Include(questions => questions.Answers)
                .AsSplitQuery()
                .FirstOrDefault(questions => questions.QuestionId == currentQuestionId);
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

        private static Result<T> GetMemberDoesNotExistError<T>(string username) =>
            new Result<T>(new ErrorMessage(HttpStatusCode.NotFound,
                    $"Member with username '{username}' does not exist."))
                .AddLink(FourWeekChallengeEndpoint.InitialiseGameRoute);

        private static Result GetMemberDoesNotExistError(string username) =>
            new Result(new ErrorMessage(HttpStatusCode.NotFound,
                    $"Member with username '{username}' does not exist."))
                .AddLink(FourWeekChallengeEndpoint.InitialiseGameRoute);

        private static bool HasGameEnded(Members member) =>
            member.QuestionIds.Split(",").Length == member.CurrentQuestionNumber;
    }
}
