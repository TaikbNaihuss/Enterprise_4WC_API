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
using Assignment4WC.Models;

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
        [Route(FourWeekChallengeEndpoint.StartRoute)]
        public IActionResult Start(int appId, [FromBody] InitialDetailsDto initialDetails)
        {
            if (appId < 0) return BadRequest($"'{nameof(appId)}' cannot be less than 0.");
            var (category, numOfQuestions, username) = initialDetails;

            var result = _manager.AddNewPlayer(appId, username, Enum.Parse<CategoryType>(category, true), numOfQuestions);

            return result.IsSuccess
                ? Created(HttpContext.Request.Path, new LinkReferencer()
                    .AddLink(FourWeekChallengeEndpoint.InterpolatedStartRoute(appId.ToString()))
                    .GetLinks()) 
                : result.AddLink(FourWeekChallengeEndpoint.InterpolatedStartRoute(appId.ToString()))
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
                .AddLink(FourWeekChallengeEndpoint.InterpolatedSubmitAnswerRoute(username))
                .GetValueAndLinks()
                .ToActionResult(this);
        }

        [HttpPost]
        [Route(FourWeekChallengeEndpoint.SubmitAnswerRoute)]
        public IActionResult SubmitAnswer(string username, [FromBody] AnswerDto answerContainer)
        {
            if (string.IsNullOrWhiteSpace(answerContainer.Answer)) 
                return new Result(new ErrorMessage(HttpStatusCode.BadRequest, "No answer was provided."))
                    .AddLink(FourWeekChallengeEndpoint.InterpolatedSubmitAnswerRoute(username))
                    .GetErrorAndLinks()
                    .ToActionResult(this);

            var result = _manager.SubmitAnswer(username, answerContainer.Answer);

            return result.IsSuccess ?
                    result.GetValueAndLinks().ToActionResult(this) :
                    result.GetErrorAndLinks().ToActionResult(this);
        }

        [HttpPost]
        [Route(FourWeekChallengeEndpoint.GetNextQuestionRoute)]
        public IActionResult GetNextQuestion(string username, string answer)
        {
            return Ok();
        }

        [HttpPut]
        [Route(FourWeekChallengeEndpoint.GetUserLocationRoute)]
        public IActionResult GetUserLocation(string username /*Also need longitude and latitude here*/)
        {
            return NoContent();
        }

        [HttpGet]
        [Route(FourWeekChallengeEndpoint.GetHintRoute)]
        public IActionResult GetHint(string username)
        {
            return Ok();
        }

        [HttpGet]
        [Route(FourWeekChallengeEndpoint.GetLocationHintRoute)]
        public IActionResult GetLocationHint(string username)
        {
            return Ok();
        }

        [HttpPut]
        [Route(FourWeekChallengeEndpoint.EndGameRoute)]
        public IActionResult EndGame(string username)
        {
            return Ok();
        }

        [HttpGet]
        [Route(FourWeekChallengeEndpoint.GetUserScoreRoute)]
        public IActionResult GetUserScore(string username)
        {
            return Ok();
        }

        [HttpGet]
        [Route(FourWeekChallengeEndpoint.GetHighScoresRoute)]
        public IActionResult GetHighScores()
        {
            return Ok();
        }
    }
}
