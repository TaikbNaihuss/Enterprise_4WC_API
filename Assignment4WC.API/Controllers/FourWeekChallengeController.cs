using Assignment4WC.API.Controllers.Models;
using Assignment4WC.API.Extensions;
using Assignment4WC.Context;
using Assignment4WC.Logic;
using Assignment4WC.Models.ControllerEndpoints;
using Assignment4WC.Models.ResultType;
using Assignment4WC.Models.ResultType.LinkReferencerObjects;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq.Expressions;
using System.Net;
using Assignment4WC.Context.Models;
using Assignment4WC.Models;
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
        [Route(FourWeekChallengeEndpoint.InitialiseGameRoute)]
        public IActionResult InitialiseGame()
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
                    .AddLink(FourWeekChallengeEndpoint.StartRouteWith(appId.ToString()))
                    .GetLinks()) 
                : result.AddLink(FourWeekChallengeEndpoint.StartRouteWith(appId.ToString()))
                    .GetErrorAndLinks()
                    .ToActionResult(this);
        }


        //[HttpPost]
        //[Route(FourWeekChallengeEndpoint."setQuestions")]
        //public IActionResult SetQuestion(string question, string ans1, string ans2, string ans3, string ans4, string ansCorrect)
        //{
        //    _context.Questions.Add(new Questions()
        //    {
        //        Question = question,
        //        CorrectAnswer = ansCorrect,
        //        CategoryId = _context.Categories.First(categories => categories.CategoryName == CategoryType.Baby.ToString()).CategoryId,
        //    });

        //    _context.SaveChanges();

        //    _context.Answers.Add(new Answers()
        //    {
        //        QuestionId = _context.Questions.First(questions => questions.Question == question).QuestionId,
        //        Answer = ans1,
        //        Order = 'A'
        //    });

        //    _context.Answers.Add(new Answers()
        //    {
        //        QuestionId = _context.Questions.First(questions => questions.Question == question).QuestionId,
        //        Answer = ans2,
        //        Order = 'B'
        //    });

        //    _context.Answers.Add(new Answers()
        //    {
        //        QuestionId = _context.Questions.First(questions => questions.Question == question).QuestionId,
        //        Answer = ans3,
        //        Order = 'C'
        //    });

        //    _context.Answers.Add(new Answers()
        //    {
        //        QuestionId = _context.Questions.First(questions => questions.Question == question).QuestionId,
        //        Answer = ans4,
        //        Order = 'D'
        //    });

        //    _context.SaveChanges();

        //    return Ok();
        //}


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
            
            return new Result<QuestionAndAnswersDto>(
                    new QuestionAndAnswersDto(
                        questionData.Question,
                        QuestionsExtensions.GetAnswersFromQuestionData(questionData)))
                .AddLink("submit",FourWeekChallengeEndpoint.SubmitAnswerRouteWith(username))
                .GetValueAndLinks()
                .ToActionResult(this);
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
                        .AddLink("initialise", FourWeekChallengeEndpoint.InitialiseGameRoute)
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
