#nullable enable
using Assignment4WC.Context.Models;
using Assignment4WC.Context.Repositories;
using Assignment4WC.Models;
using Assignment4WC.Models.ControllerEndpoints;
using Assignment4WC.Models.ResultType;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;

namespace Assignment4WC.Logic
{
    public class FourWeekChallengeManager : IFourWeekChallengeManager
    {
        private readonly IGlobalRepository _repository;
        private readonly IQuestionRandomiser _questionRandomiser;

        public FourWeekChallengeManager(IGlobalRepository repository, IQuestionRandomiser questionRandomiser)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _questionRandomiser = questionRandomiser ?? throw new ArgumentNullException(nameof(questionRandomiser));
        }

        public Result<List<CategoryWithQuestionCount>> GetCategoriesAndQuestionCount() =>
            new Result<List<CategoryWithQuestionCount>>(
                    Enum.GetNames(typeof(CategoryType))
                        .Select(categoryString => (CategoryType)Enum.Parse(typeof(CategoryType), categoryString))
                        .Select(category => new CategoryWithQuestionCount(
                            category.ToString(),
                            GetQuestionCountInIncrements(category, 5)))
                        .ToList())
                .AddLink(FourWeekChallengeEndpoint.StartRoute);

        public Result UpdateUserLocation(string username, decimal latitude, decimal longitude)
        {
            var member = _repository.Members.GetMemberOrNull(username);
            if (member == null)
                return GetMemberDoesNotExistError(username);

            var newLocation = new Locations()
            {
                Latitude = latitude,
                Longitude = longitude
            };

            if (member.LocationId == 0)
            {
                _repository.Locations.Add(newLocation);
                _repository.SaveChanges();
                var location = _repository.Locations.GetLocationByLocation(newLocation);
                member.LocationId = location.LocationId;
            }
            else
            {
                var userLocation = _repository.Locations.GetLocationByLocationId(member.LocationId);
                userLocation.Latitude = latitude;
                userLocation.Longitude = longitude;
            }

            _repository.SaveChanges();

            return new Result().Ok();
        }

        public Result<string> GetHintFromQuestion(string username)
        {
            var questionDataResult = GetCurrentComplexQuestionData(username);
            if (!questionDataResult.IsSuccess)
                return questionDataResult.ToResult<string>();

            _repository.Members.GetMemberOrNull(username)!.HintAsked = true;
            _repository.SaveChanges();

            return new Result<string>(questionDataResult.Unwrap().Hint);
        }

        public Result AddNewPlayer(int appId, string username, CategoryType category, int numOfQuestions)
        {
            if (_repository.Members.DoesUsernameExist(username))
                return new Result(new ErrorMessage(HttpStatusCode.BadRequest,
                    $"The username '{username}' already exists, try another."))
                    .AddLink(FourWeekChallengeEndpoint.StartRouteWith(appId.ToString()));

            var result = _questionRandomiser.GetQuestionsWithOrder(numOfQuestions, category);
            if (!result.IsSuccess) return new Result(result.GetError())
                .AddLink(FourWeekChallengeEndpoint.StartRouteWith(appId.ToString()));

            _repository.Members.Add(new Members
            {
                AppId = appId,
                Username = username,
                QuestionIds = result.Unwrap(),
            });

            _repository.SaveChanges();

            return new Result().Ok();
        }

        public Result<Questions> GetCurrentQuestionData(string username)
        {
            var currentQuestionIdResult = GetMembersCurrentQuestionId(username);
            if (!currentQuestionIdResult.IsSuccess)
                return currentQuestionIdResult.ToResult<Questions>();

            var currentQuestionId = currentQuestionIdResult.Unwrap();

            var question = _repository.Questions.GetQuestionOrNull(currentQuestionId);
           
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
                        .AddLink("setLocation", FourWeekChallengeEndpoint.SetUserLocationRouteWith(username))
                        .AddLink("locationHint", FourWeekChallengeEndpoint.GetLocationHintRouteWith(username));

            return result;
        }

        public Result<string> GetLocationHintFromQuestion(string username)
        {
            var questionDataResult = GetCurrentComplexQuestionData(username);
            if (!questionDataResult.IsSuccess)
                return questionDataResult.ToResult<string>();

            _repository.Members.GetMemberOrNull(username)!.LocationHintAsked = true;
            _repository.SaveChanges();

            return questionDataResult.HasValue()
                ? new Result<string>(questionDataResult.Unwrap().LocationHint)
                : new Result<string>(string.Empty);
        }

        public Result EndGame(string username)
        {
            var member = _repository.Members.GetMemberOrNull(username);
            if (member == null)
                return GetMemberDoesNotExistError(username);

            return HasGameEnded(member) ? 
                new Result().Ok() : 
                new Result(new ErrorMessage(HttpStatusCode.BadRequest,
                    $"Game has not ended for member with name '{username}.'"))
                    .AddLink(FourWeekChallengeEndpoint.GetQuestionRouteWith(username));

        }

        public Result<int> GetUserScore(string username)    
        {
            var member = _repository.Members.GetMemberOrNull(username);
            return member != null ? 
                new Result<int>(member.UserScore) 
                    .AddLink("category", FourWeekChallengeEndpoint.GetCategories)
                    .AddLink("highScore", FourWeekChallengeEndpoint.GetHighScoresRoute)
                : GetMemberDoesNotExistError(username)
                    .ToResult<int>();
        }

        public Result<List<UserScore>> GetHighScores()
        {
            if (!_repository.Members.Any())
                return new Result<List<UserScore>>(
                        new ErrorMessage(HttpStatusCode.BadRequest, "No members currently exist."))
                    .AddLink("categories", FourWeekChallengeEndpoint.GetCategories);
            
            return new Result<List<UserScore>>(_repository.Members.GetUserScoreInDescendingOrder())
                .AddLink("categories", FourWeekChallengeEndpoint.GetCategories);
        }

        public Result<bool> SubmitPictureAnswer(string username, IFormFile picture)
        {
            var extension = picture.FileName.Split(".").ToList().Last();
            if (extension is not ("jpg" or "png" or "jpeg"))
                return new Result<bool>(new ErrorMessage(HttpStatusCode.UnprocessableEntity,
                            $"File types are limited to '.jpg','.jpeg' and '.png'. The uploaded file type was .{extension}"))
                        .AddLink(FourWeekChallengeEndpoint.SubmitPictureAnswerRouteWith(username));

            var pictureBase64 = "";

            if (picture.Length > 0)
            {
                using var ms = new MemoryStream();
                picture.CopyTo(ms);
                var fileBytes = ms.ToArray();
                pictureBase64 = Convert.ToBase64String(fileBytes);
            }

            return SubmitAnswer(username, pictureBase64);
        }

        public Result<bool> SubmitAnswer(string username, string answer)
        {
            var questionDataResult = GetCurrentQuestionData(username);
            if (!questionDataResult.IsSuccess)
                return questionDataResult.ToResult<bool>();

            var member = _repository.Members.GetMemberOrNull(username)!;

            var questionData = questionDataResult.Unwrap();
            
            var complexQuestionData = questionData.Discriminator == QuestionComplexity.Simple.ToString() ?
                    null :
                    _repository.ComplexQuestions.GetComplexQuestion(questionData.QuestionId);

            var skippedQuestion = string.Equals(answer, "PASS", StringComparison.CurrentCultureIgnoreCase);

            var isSameLocation = true;

            if (questionData.CorrectAnswer == answer || skippedQuestion)
            {
                if (!skippedQuestion)
                {
                    if (complexQuestionData != null)
                    {
                        var sameLocationsResult = AreLocationsCloseEnough(complexQuestionData, member);
                        if (!sameLocationsResult.IsSuccess)
                            return sameLocationsResult;

                        isSameLocation = sameLocationsResult.Unwrap();
                    }

                    if (isSameLocation)
                    {
                        AddToUserScore(member);
                        member.CurrentQuestionNumber++;
                    }
                }
                else
                    member.CurrentQuestionNumber++;

                _repository.SaveChanges();
            }

            var result = new Result<bool>((questionData.CorrectAnswer == answer && isSameLocation) || skippedQuestion);

            return !HasGameEnded(member) ?
                result.AddLink(FourWeekChallengeEndpoint.GetQuestionRouteWith(username)) :
                result.AddLink(FourWeekChallengeEndpoint.EndGameRouteWith(username));
        }

        private Result<bool> AreLocationsCloseEnough(ComplexQuestions complexQuestionData, Members member)
        {
            const int correctLocationRadius = 20;
            var complexQuestionLocation = complexQuestionData.Location;
            var memberLocation = _repository.Locations.GetLocationByLocationIdOrNull(member.LocationId);

            if (memberLocation == null)
                return new Result<bool>(
                        new ErrorMessage(HttpStatusCode.NotFound,
                            $"Member with username '{member.Username}' does not have a location set."))
                    .AddLink(FourWeekChallengeEndpoint.SetUserLocationRouteWith(member.Username));

            var withinLocationRadius = memberLocation.GetDistanceInMeters(complexQuestionLocation) <= correctLocationRadius;

            return new Result<bool>(withinLocationRadius);
        }

        private IEnumerable<int> GetQuestionCountInIncrements(CategoryType category, int increments)
        {
            var questionCount = _repository.Questions.CountQuestionsFromCategory(category);

            for (var i = 0; i < questionCount / increments; i++)
            {
                yield return increments * (i + 1);
            }
        }

        private Result<ComplexQuestions> GetCurrentComplexQuestionData(string username)
        {
            var currentQuestionIdResult = GetMembersCurrentQuestionId(username);
            if (!currentQuestionIdResult.IsSuccess)
                return currentQuestionIdResult.ToResult<ComplexQuestions>();

            var currentQuestionId = currentQuestionIdResult.Unwrap();

            var complexQuestion = _repository.ComplexQuestions.GetComplexQuestion(currentQuestionId);

            var questionExists = _repository.Questions.DoesQuestionExist(currentQuestionId);

            return complexQuestion != null ?
                new Result<ComplexQuestions>(complexQuestion) :
                questionExists ?
                    new Result<ComplexQuestions>(new ErrorMessage(HttpStatusCode.BadRequest,
                            $"This question is not a complex question. Cannot provide additional details for this question."))
                        .AddLink(FourWeekChallengeEndpoint.GetQuestionRouteWith(username)) :
                    new Result<ComplexQuestions>(new ErrorMessage(HttpStatusCode.NotFound,
                            $"Question with ID '{currentQuestionId}' does not exist in database."))
                        .AddLink(FourWeekChallengeEndpoint.GetQuestionRouteWith(username));
        }

        private Result<int> GetMembersCurrentQuestionId(string username)
        {
            var member = _repository.Members.GetMemberOrNull(username);
            if (member == null)
                return GetMemberDoesNotExistError(username)
                    .ToResult<int>();

            var currentQuestionIdResult = GetCurrentQuestionId(member);

            return currentQuestionIdResult.IsSuccess ?
                currentQuestionIdResult :
                currentQuestionIdResult.ToResult<int>();
        }

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

            return questionIds.Length != currentQuestionIndex ? 
                new Result<int>(int.Parse(questionIds[currentQuestionIndex])) :
                new Result<int>(new ErrorMessage(HttpStatusCode.NotFound, $"Game has ended for member with name '{member.Username}'."))
                    .AddLink(FourWeekChallengeEndpoint.EndGameRouteWith(member.Username));
        }


        private static Result GetMemberDoesNotExistError(string username) =>
            new Result(new ErrorMessage(HttpStatusCode.NotFound,
                    $"Member with username '{username}' does not exist."))
                .AddLink(FourWeekChallengeEndpoint.GetCategories);

        private static bool HasGameEnded(Members member) =>
            member.QuestionIds.Split(",").Length == member.CurrentQuestionNumber;
    }
}
