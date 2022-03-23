using Assignment4WC.API.Controllers.Models;
using Assignment4WC.API.Extensions;
using Assignment4WC.Logic;
using Assignment4WC.Models;
using Assignment4WC.Models.ControllerEndpoints;
using Assignment4WC.Models.ResultType;
using Assignment4WC.Models.ResultType.LinkReferencerObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Assignment4WC.API.Controllers
{
    [ApiController]
    [Route("[controller]/api")]
    public class FourWeekChallengeController : Controller
    {
        private readonly IFourWeekChallengeManager _manager;

        public FourWeekChallengeController(IFourWeekChallengeManager manager)
        {
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
        }

        [HttpPost]
        [Route(FourWeekChallengeEndpoint.GetCategories)]
        public IActionResult GetCategories() => 
            _manager.GetCategoriesAndQuestionCount()
                .GetValueAndLinks()
                .ToActionResult(this);

        [HttpPost]
        [Route(FourWeekChallengeEndpoint.StartRoute)]
        public IActionResult StartGame(int appId, [FromBody] InitialDetailsDto initialDetails)
        {
            //If the appID is less than zero, return a bad request with an error.
            if (appId < 0) return BadRequest($"'{nameof(appId)}' cannot be less than 0.");
            var (category, numOfQuestions, username) = initialDetails;

            //Add the new player.
            var result = _manager.AddNewPlayer(appId, username, Enum.Parse<CategoryType>(category, true), numOfQuestions);

            //If the operation was successful, return a Created status code with appropriate link(s),
            //otherwise return the errors and links.
            return result.IsSuccess
                ? Created(HttpContext.Request.Path, new LinkReferencer()
                    .AddLink(FourWeekChallengeEndpoint.GetQuestionRouteWith(username))
                    .GetLinks()) 
                : result.GetErrorAndLinks()
                    .ToActionResult(this);
        }

        [HttpGet]
        [Route(FourWeekChallengeEndpoint.GetQuestionRoute)]
        public IActionResult GetQuestion(string username)
        {
            //Get the current question data for the possible member.
            //If an error occurs in that process, return the error with an appropriate link.
            var questionDataResult = _manager.GetCurrentQuestionData(username);
            if (!questionDataResult.IsSuccess) 
                return questionDataResult
                    .GetErrorAndLinks()
                    .ToActionResult(this);

            //Get the value from the result.
            var questionData = questionDataResult.Unwrap();
            
            //If there is questionData, given its multiple choice, add all applicable answers associated with the answer.
            //Any other type of question can just take the question data and disregard the answer data.
            //If no question data was return, the game has ended and just append appropriate link(s) to the OK status code result.
            return questionData != null ?
                questionData.QuestionType == QuestionType.MultipleChoice ?
                    AppendValueLinkData(new Result<QuestionAndAnswersDto>(
                        new QuestionAndAnswersDto(
                            questionData.Question,
                            QuestionsExtensions.GetAnswersFromQuestionData(questionData))),
                        questionData.QuestionType) :
                    AppendValueLinkData(new Result<QuestionDto>(
                        new QuestionDto(
                            questionData.Question)), 
                        questionData.QuestionType) :
                Ok(new LinkReferencer()
                    .AddLink(FourWeekChallengeEndpoint.EndGameRouteWith(username))
                    .GetLinks());

            //Appends specific links to the submit endpoints given the question is a picture,
            //it also appends the original links from the result evaluated prior.
            IActionResult AppendValueLinkData<T>(Result<T> result, QuestionType questionType)
            {
                return (questionType != QuestionType.Picture ?
                    result.AddLink("submit", FourWeekChallengeEndpoint.SubmitAnswerRouteWith(username)) :
                    result.AddLink("submit", FourWeekChallengeEndpoint.SubmitPictureAnswerRouteWith(username)))
                    .WithLinks(questionDataResult.GetValueAndLinks())
                    .GetValueAndLinks()
                    .ToActionResult(this);
            }
        }

        [HttpPost]
        [Route(FourWeekChallengeEndpoint.SubmitAnswerRoute)]
        public IActionResult SubmitAnswer(string username, [FromBody] AnswerDto answerContainer)
        {
            //Submits the users answer and return a result after the process is finished.
            var result = _manager.SubmitAnswer(username, answerContainer.Answer);

            //If the result is a success, give the value data, otherwise the error data.
            return result.IsSuccess ?
                    result.GetValueAndLinks().ToActionResult(this):
                    result.GetErrorAndLinks().ToActionResult(this);
        }

        [HttpPost]
        [Route(FourWeekChallengeEndpoint.SubmitPictureAnswerRoute)]
        public IActionResult SubmitPictureAnswer(string username, IFormFile picture)
        {
            //Submits the users picture answer and return a result after the process is finished.
            var result = _manager.SubmitPictureAnswer(username, picture);

            //If the result is a success, give the value data, otherwise the error data.
            return result.IsSuccess ?
                result.GetValueAndLinks().ToActionResult(this) :
                result.GetErrorAndLinks().ToActionResult(this);
        }

        [HttpPut]
        [Route(FourWeekChallengeEndpoint.SetUserLocationRoute)]
        public IActionResult SetUserLocation(string username, LocationDto location)
        {
            //Updates the users location and return a result after the process is finished.
            var result = _manager.UpdateUserLocation(username, location.Latitude, location.Longitude);

            //If the result is a success, give the value data, otherwise the error data.
            return result.IsSuccess ?
                NoContent() :
                result.GetErrorAndLinks().ToActionResult(this);
        }

        [HttpGet]
        [Route(FourWeekChallengeEndpoint.GetHintRoute)]
        public IActionResult GetHint(string username)
        {
            //Gets a possible hint for the user and return a result after the process is finished.
            var result = _manager.GetHintFromQuestion(username);

            //If the result is a success, give the value data, otherwise the error data.
            return result.IsSuccess ?
                result.GetValueAndLinks().ToActionResult(this):
                result.GetErrorAndLinks().ToActionResult(this);
        }

        [HttpGet]
        [Route(FourWeekChallengeEndpoint.GetLocationHintRoute)]
        public IActionResult GetLocationHint(string username)
        {
            //Gets a possible location hint for the user and return a result after the process is finished.
            var result = _manager.GetLocationHintFromQuestion(username);

            //If the result is a success, give the value data, otherwise the error data.
            return result.IsSuccess ?
                result.GetValueAndLinks().ToActionResult(this) :
                result.GetErrorAndLinks().ToActionResult(this);
        }

        [HttpPut]
        [Route(FourWeekChallengeEndpoint.EndGameRoute)]
        public IActionResult EndGame(string username)
        {
            //Checks if the game has ended for the user and return a result after the process is finished.
            var result = _manager.EndGame(username);

            //If the result is a success, return an OK status code result with appropriate link(s),
            //otherwise return the error and its links.
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
            //Gets a possible members user score and return a result after the process is finished.
            var result = _manager.GetUserScore(username);

            //If the result is a success, give the value data, otherwise the error data.
            return result.IsSuccess ?
                result.GetValueAndLinks().ToActionResult(this) :
                result.GetErrorAndLinks().ToActionResult(this);
        }

        [HttpGet]
        [Route(FourWeekChallengeEndpoint.GetHighScoresRoute)]
        public IActionResult GetHighScores()
        {
            //Gets all high scores from all registered members and return a result after the process is finished.
            var result = _manager.GetHighScores();

            //If the result is a success, give the value data, otherwise the error data.
            return result.IsSuccess ?
                result.GetValueAndLinks().ToActionResult(this) :
                result.GetErrorAndLinks().ToActionResult(this);
        }
    }
}
