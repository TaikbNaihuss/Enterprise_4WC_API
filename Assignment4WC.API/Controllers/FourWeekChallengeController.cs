using Assignment4WC.API.Controllers.Models;
using Assignment4WC.API.Extensions;
using Assignment4WC.Context;
using Assignment4WC.Context.Models;
using Assignment4WC.Logic;
using Assignment4WC.Models;
using Assignment4WC.Models.ControllerEndpoints;
using Assignment4WC.Models.ResultType;
using Assignment4WC.Models.ResultType.LinkReferencerObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Net;
using LocationDto = Assignment4WC.API.Controllers.Models.LocationDto;

namespace Assignment4WC.API.Controllers
{
    [ApiController]
    [Route("[controller]/api")]
    public class FourWeekChallengeController : Controller
    {
        private readonly IFourWeekChallengeManager _manager;
        private readonly AssignmentContext _context;

        public FourWeekChallengeController(IFourWeekChallengeManager manager, AssignmentContext context)
        {
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
            _context = context;
        }

        [HttpPost]
        [Route(FourWeekChallengeEndpoint.GetCategories)]
        public IActionResult GetCategories()
        {
            var result = _manager.GetCategoriesAndQuestionCount();

            return result.IsSuccess ? 
                result.GetValueAndLinks().ToActionResult(this) :
                result.GetErrorAndLinks().ToActionResult(this);
        }       

        [HttpPost]
        [Route(FourWeekChallengeEndpoint.StartRoute)]
        public IActionResult StartGame(int appId, [FromBody] InitialDetailsDto initialDetails)
        {
            if (appId < 0) return BadRequest($"'{nameof(appId)}' cannot be less than 0.");
            var (category, numOfQuestions, username) = initialDetails;

            var result = _manager.AddNewPlayer(appId, username, Enum.Parse<CategoryType>(category, true), numOfQuestions);

            return result.IsSuccess
                ? Created(HttpContext.Request.Path, new LinkReferencer()
                    .AddLink(FourWeekChallengeEndpoint.GetQuestionRouteWith(username))
                    .GetLinks()) 
                : result.AddLink(FourWeekChallengeEndpoint.StartRouteWith(appId.ToString()))
                    .GetErrorAndLinks()
                    .ToActionResult(this);
        }


        [HttpPost]
        [Route("setQuestions")]
        public IActionResult SetQuestion(string question, string answer, string hint)
        {
            var random = new Random(3);

            var location = new Locations()
            {
                Latitude = Convert.ToDecimal(random.NextDouble() * 80),
                Longitude = Convert.ToDecimal(random.NextDouble() * 80)
            };

            _context.Locations.Add(location);

            _context.SaveChanges();

            _context.Questions.Add(new ComplexQuestions()
            {
                Question = question,
                CorrectAnswer = answer,
                CategoryId = _context.Categories.First(categories => categories.CategoryName == CategoryType.Animal).CategoryId,
                Hint = hint,
                LocationHint = hint + "Location",
                LocationId = _context.Locations.First(locations => locations == location).LocationId,
                QuestionType = QuestionType.Text
            });

            _context.SaveChanges();

            return Ok();
        }


        [HttpGet]
        [Route(FourWeekChallengeEndpoint.GetQuestionRoute)]
        public IActionResult GetQuestion(string username)
        {
            var questionDataResult = _manager.GetCurrentQuestionData(username);
            if (!questionDataResult.IsSuccess) 
                return questionDataResult
                    .GetErrorAndLinks()
                    .ToActionResult(this);

            var questionData = questionDataResult.Unwrap();
            
            return questionData != null ?
                questionData.QuestionType == QuestionType.MultipleChoice ?
                    AppendValueLinkData(new Result<QuestionAndAnswersDto>(
                        new QuestionAndAnswersDto(
                            questionData.Question,
                            QuestionsExtensions.GetAnswersFromQuestionData(questionData)))) :
                    AppendValueLinkData(new Result<QuestionDto>(
                        new QuestionDto(
                            questionData.Question))) :
                Ok(new LinkReferencer()
                    .AddLink(FourWeekChallengeEndpoint.EndGameRouteWith(username))
                    .GetLinks());

            IActionResult AppendValueLinkData<T>(Result<T> result)
            {
                return result.AddLink("submit", FourWeekChallengeEndpoint.SubmitAnswerRouteWith(username))
                    .GetValueAndLinks()
                    .ToActionResult(this);
            }
        }

        [HttpPost]
        [Route(FourWeekChallengeEndpoint.SubmitAnswerRoute)]
        public IActionResult SubmitAnswer(string username, [FromBody] AnswerDto answerContainer)
        {
            if (string.IsNullOrWhiteSpace(answerContainer.Answer)) 
                return new Result(new ErrorMessage(HttpStatusCode.BadRequest, "No answer was provided."))
                    .AddLink(FourWeekChallengeEndpoint.SubmitAnswerRouteWith(username))
                    .GetErrorAndLinks()
                    .ToActionResult(this);

            var result = _manager.SubmitAnswer(username, answerContainer.Answer);

            return result.IsSuccess ?
                    result.GetValueAndLinks().ToActionResult(this):
                    result.GetErrorAndLinks().ToActionResult(this);
        }

        [HttpPut]
        [Route(FourWeekChallengeEndpoint.SetUserLocationRoute)]
        public IActionResult SetUserLocation(string username, LocationDto location)
        {
            var result = _manager.UpdateUserLocation(username, location.Latitude, location.Longitude);

            return result.IsSuccess ?
                NoContent() :
                result.GetErrorAndLinks().ToActionResult(this);
        }

        [HttpGet]
        [Route(FourWeekChallengeEndpoint.GetHintRoute)]
        public IActionResult GetHint(string username)
        {
            var result = _manager.GetHintFromQuestion(username);

            return result.IsSuccess ?
                result.GetValueAndLinks().ToActionResult(this):
                result.GetErrorAndLinks().ToActionResult(this);
        }

        [HttpGet]
        [Route(FourWeekChallengeEndpoint.GetLocationHintRoute)]
        public IActionResult GetLocationHint(string username)
        {
            var result = _manager.GetLocationHintFromQuestion(username);
           
            return result.IsSuccess ?
                result.GetValueAndLinks().ToActionResult(this) :
                result.GetErrorAndLinks().ToActionResult(this);
        }

        [HttpPut]
        [Route(FourWeekChallengeEndpoint.EndGameRoute)]
        public IActionResult EndGame(string username)
        {
            var result = _manager.EndGame(username);

            return result.IsSuccess ?
                Ok(new LinkReferencer()
                        .AddLink("categories", FourWeekChallengeEndpoint.GetCategories)
                        .AddLink("score", FourWeekChallengeEndpoint.GetUserScoreRouteWith(username))
                        .AddLink("highScores", FourWeekChallengeEndpoint.GetHighScoresRoute)
                        .GetLinks()):
                result.GetErrorAndLinks().ToActionResult(this);
        }

        [HttpGet]
        [Route(FourWeekChallengeEndpoint.GetUserScoreRoute)]
        public IActionResult GetUserScore(string username)
        {
            var result = _manager.GetUserScore(username);
            return result.IsSuccess ?
                result.GetValueAndLinks().ToActionResult(this) :
                result.GetErrorAndLinks().ToActionResult(this);
        }

        [HttpGet]
        [Route(FourWeekChallengeEndpoint.GetHighScoresRoute)]
        public IActionResult GetHighScores()
        {
            var result = _manager.GetHighScores();
            
            return result.IsSuccess ?
                result.GetValueAndLinks().ToActionResult(this) :
                result.GetErrorAndLinks().ToActionResult(this);
        }
    }
}
