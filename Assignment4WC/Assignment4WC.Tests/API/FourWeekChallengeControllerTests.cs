#nullable enable
using Assignment4WC.API.Controllers;
using Assignment4WC.API.Controllers.Models;
using Assignment4WC.API.Extensions;
using Assignment4WC.Context;
using Assignment4WC.Context.Models;
using Assignment4WC.Logic;
using Assignment4WC.Models;
using Assignment4WC.Models.ControllerEndpoints;
using Assignment4WC.Models.ResultType;
using Assignment4WC.Models.ResultType.LinkReferencerObjects;
using AutoFixture;
using AutoFixture.Xunit2;
using FluentAssertions;
using FluentAssertions.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Moq;
using System;
using System.Collections.Generic;
using System.Net;
using Xunit;

namespace Assignment4WC.Tests.API
{
    public class FourWeekChallengeControllerTests : IDisposable
    {
        protected readonly AssignmentContext Context;
        protected readonly IFixture Fixture;
        protected FourWeekChallengeControllerTests()
        {
            Context = new AssignmentContext(AssignmentContextOptions.GetDbOptions());
            Fixture = new Fixture();
            Fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
            Fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        public class GetCategoriesTests : FourWeekChallengeControllerTests
        {
            [Fact]
            public void GivenThereAreCategoriesWithQuestions_ThenReturnCategoriesWithQuestionCount()
            {
                var managerMock = new Mock<IFourWeekChallengeManager>();

                var expectedValue = new Result<List<CategoryWithQuestionCount>>(
                    new List<CategoryWithQuestionCount>
                    {
                        Fixture.Build<CategoryWithQuestionCount>()
                            .With(c => c.QuestionCount,
                                () => Fixture.CreateMany<int>(5)).Create()
                    });

                managerMock
                    .Setup(manager => manager.GetCategoriesAndQuestionCount())
                    .Returns(() => expectedValue);

                new FourWeekChallengeController(managerMock.Object)
                    .GetCategories()
                    .Should().BeOkObjectResult()
                    .WithValueEquivalentTo(expectedValue.GetValueAndLinks());
            }
        }

        public class StartGameTests : FourWeekChallengeControllerTests
        {
            [Theory]
            [InlineData(-1)]
            [InlineData(-24)]
            [InlineData(-55645)]
            [InlineData(-42465556)]
            [InlineData(int.MinValue)]
            public void GivenAppIdIsLessThanZero_ThenReturnBadRequestErrorResult(int appId)
            {
                var initialDetailsDto = GetValidInitialDetailsDto();
                var expectedValue = $"'{nameof(appId)}' cannot be less than 0.";

                new FourWeekChallengeController(new Mock<IFourWeekChallengeManager>().Object)
                    .StartGame(appId, initialDetailsDto)
                    .Should().BeBadRequestObjectResult()
                    .WithValueEquivalentTo(expectedValue);
            }

            [Theory]
            [AutoData]
            public void GivenAddNewPlayerFailed_ThenReturnBadRequestErrorResultWithLinks(int appId, string errorMessage)
            {
                var managerMock = new Mock<IFourWeekChallengeManager>();

                var initialDetailsDto = GetValidInitialDetailsDto();
                var expectedValue = new Result(new ErrorMessage(HttpStatusCode.BadRequest, errorMessage));

                SetupManagerMockAddNewPlayer(managerMock, appId, initialDetailsDto, expectedValue);

                new FourWeekChallengeController(managerMock.Object)
                    .StartGame(appId, initialDetailsDto)
                    .Should().BeBadRequestObjectResult()
                    .WithValueEquivalentTo(expectedValue.GetErrorAndLinks());
            }

            [Theory]
            [AutoData]
            public void GivenAddNewPlayerIsSuccessful_ThenReturnSuccessWithLinks(int appId)
            {
                var managerMock = new Mock<IFourWeekChallengeManager>();
                var httpContextMock = new Mock<HttpContext>();

                var initialDetailsDto = GetValidInitialDetailsDto();

                var expectedValue = new LinkReferencer()
                    .AddLink(FourWeekChallengeEndpoint.GetQuestionRouteWith(initialDetailsDto.Username))
                    .GetLinks();

                SetupManagerMockAddNewPlayer(managerMock, appId, initialDetailsDto, new Result().Ok());

                httpContextMock.SetupGet(context => context.Request.Path)
                    .Returns(() => $"/{Fixture.Create<string>()}");

                var controller = new FourWeekChallengeController(managerMock.Object);
                controller.ControllerContext = new ControllerContext(
                    new ActionContext(httpContextMock.Object, new RouteData(), new ControllerActionDescriptor()));

                controller
                    .StartGame(appId, initialDetailsDto)
                    .Should().BeCreatedResult()
                    .WithValueEquivalentTo(expectedValue);
            }

            private static void SetupManagerMockAddNewPlayer(Mock<IFourWeekChallengeManager> managerMock,
                int appId, InitialDetailsDto initialDetailsDto, Result returnVal)
            {
                managerMock
                    .Setup(manager => manager.AddNewPlayer(
                        appId,
                        initialDetailsDto.Username,
                        Enum.Parse<CategoryType>(initialDetailsDto.Category),
                        initialDetailsDto.NumOfQuestions))
                    .Returns(() => returnVal);
            }

            private InitialDetailsDto GetValidInitialDetailsDto()
            {
                return Fixture.Build<InitialDetailsDto>()
                    .With(dto => dto.Category, () => It.IsAny<CategoryType>().ToString())
                    .Create();
            }
        }

        public class GetQuestionTests : FourWeekChallengeControllerTests
        {
            [Fact]
            public void GivenGetCurrentQuestionDataReturnsErrorResult_ThenReturnErrorResultWithLinks()
            {
                var managerMock = new Mock<IFourWeekChallengeManager>();
                var username = Fixture.Create<string>();
                var errorMessage = Fixture.Create<string>();

                var expectedValue = new Result<Questions>(new ErrorMessage(HttpStatusCode.BadRequest, errorMessage));
             
                SetupManagerMockGetCurrentQuestionData(managerMock, username, expectedValue);

                new FourWeekChallengeController(managerMock.Object)
                    .GetQuestion(username)
                    .Should().BeBadRequestObjectResult()
                    .WithValueEquivalentTo(expectedValue.GetErrorAndLinks());
            }

            [Fact]
            public void GivenCurrentQuestionDataIsNull_ThenReturnOkResultWithLinks()
            {
                var managerMock = new Mock<IFourWeekChallengeManager>();
                var username = Fixture.Create<string>();

                var expectedValue = new LinkReferencer()
                    .AddLink(FourWeekChallengeEndpoint.EndGameRouteWith(username))
                    .GetLinks();

                SetupManagerMockGetCurrentQuestionData(managerMock, username, new Result<Questions>((Questions)null!));

                new FourWeekChallengeController(managerMock.Object)
                    .GetQuestion(username)
                    .Should().BeOkObjectResult()
                    .WithValueEquivalentTo(expectedValue);
            }

            [Theory]
            [AutoData]
            public void GivenCurrentQuestionTypeIsMultipleChoice_ThenReturnOkResultWithLinks(string username)
            {
                var managerMock = new Mock<IFourWeekChallengeManager>();

                var result = new Result<Questions>(
                    Fixture.Build<Questions>()
                        .With(questions => questions.QuestionType, () => QuestionType.MultipleChoice)
                        .Create());

                var resultData = result.Unwrap();

                var expectedValue = new Result<QuestionAndAnswersDto>(new QuestionAndAnswersDto(
                            resultData.Question,
                            QuestionsExtensions.GetAnswersFromQuestionData(resultData)))
                        .AddLink("submit", FourWeekChallengeEndpoint.SubmitAnswerRouteWith(username));

                SetupManagerMockGetCurrentQuestionData(managerMock, username, result);

                new FourWeekChallengeController(managerMock.Object)
                    .GetQuestion(username)
                    .Should().BeOkObjectResult()
                    .WithValueEquivalentTo(expectedValue.GetValueAndLinks());
            }

            [Theory]
            [InlineAutoData(QuestionType.Text)]
            public void GivenCurrentQuestionTypeIsNotMultipleChoiceAndNotPicture_ThenReturnOkResultWithLinks(QuestionType questionType, string username)
            {
                var managerMock = new Mock<IFourWeekChallengeManager>();

                var result = new Result<Questions>(
                    Fixture.Build<Questions>()
                        .With(questions => questions.QuestionType, () => questionType)
                        .Create());

                var expectedValue = new Result<QuestionDto>(new QuestionDto(
                        result.Unwrap().Question))
                    .AddLink("submit", FourWeekChallengeEndpoint.SubmitAnswerRouteWith(username));

                SetupManagerMockGetCurrentQuestionData(managerMock, username, result);

                new FourWeekChallengeController(managerMock.Object)
                    .GetQuestion(username)
                    .Should().BeOkObjectResult()
                    .WithValueEquivalentTo(expectedValue.GetValueAndLinks());
            }

            [Theory]
            [AutoData]
            public void GivenCurrentQuestionTypeIsPicture_ThenReturnOkResultWithLinks(string username)
            {
                var managerMock = new Mock<IFourWeekChallengeManager>();

                var result = new Result<Questions>(
                    Fixture.Build<Questions>()
                        .With(questions => questions.QuestionType, () => QuestionType.Picture)
                        .Create());

                var expectedValue = new Result<QuestionDto>(new QuestionDto(
                        result.Unwrap().Question))
                    .AddLink("submit", FourWeekChallengeEndpoint.SubmitPictureAnswerRouteWith(username));

                SetupManagerMockGetCurrentQuestionData(managerMock, username, result);

                new FourWeekChallengeController(managerMock.Object)
                    .GetQuestion(username)
                    .Should().BeOkObjectResult()
                    .WithValueEquivalentTo(expectedValue.GetValueAndLinks());
            }

            private static void SetupManagerMockGetCurrentQuestionData(
                Mock<IFourWeekChallengeManager> managerMock, string username, Result<Questions> expectedValue)
            {
                managerMock.Setup(manager => manager.GetCurrentQuestionData(username))
                    .Returns(() => expectedValue);
            }
        }

        public class SubmitAnswerTests : FourWeekChallengeControllerTests
        {
            [Theory]
            [AutoData]
            public void GivenSubmitAnswerReturnsErrorResult_ThenReturnBadRequestResultWithErrorAndLinks
                (string username, string answer, string errorMessage)
            {
                var managerMock = new Mock<IFourWeekChallengeManager>();

                var expectedValue = new Result<bool>(new ErrorMessage(HttpStatusCode.BadRequest, errorMessage));

                SetupManagerMockSubmitAnswer(username, answer, managerMock, expectedValue);

                new FourWeekChallengeController(managerMock.Object)
                    .SubmitAnswer(username, new AnswerDto(answer))
                    .Should().BeBadRequestObjectResult()
                    .WithValueEquivalentTo(expectedValue.GetErrorAndLinks());
            }

            [Theory]
            [AutoData]
            public void GivenSubmitAnswerReturnsSuccessResult_ThenReturnOkRequestResultWithValueAndLinks(
                string username, string answer, bool resultData)
            {
                var managerMock = new Mock<IFourWeekChallengeManager>();

                var expectedValue = new Result<bool>(resultData);

                SetupManagerMockSubmitAnswer(username, answer, managerMock, expectedValue);

                new FourWeekChallengeController(managerMock.Object)
                    .SubmitAnswer(username, new AnswerDto(answer))
                    .Should().BeOkObjectResult()
                    .WithValueEquivalentTo(expectedValue.GetValueAndLinks());
            }

            private static void SetupManagerMockSubmitAnswer(string username, string answer,
                Mock<IFourWeekChallengeManager> managerMock, Result<bool> expectedValue)
            {
                managerMock.Setup(manager => manager.SubmitAnswer(username, answer))
                    .Returns(() => expectedValue);
            }
        }

        public class SubmitPictureAnswerTests : FourWeekChallengeControllerTests
        {
            [Theory]
            [AutoData]
            public void GivenSubmitPictureAnswerReturnsErrorResult_ThenReturnBadRequestResultWithErrorAndLinks
                (string username, string errorMessage)
            {
                var managerMock = new Mock<IFourWeekChallengeManager>();

                var expectedValue = new Result<bool>(new ErrorMessage(HttpStatusCode.BadRequest, errorMessage));

                SetupManagerMockSubmitPictureAnswer(username, managerMock, expectedValue);

                new FourWeekChallengeController(managerMock.Object)
                    .SubmitPictureAnswer(username, new Mock<IFormFile>().Object)
                    .Should().BeBadRequestObjectResult()
                    .WithValueEquivalentTo(expectedValue.GetErrorAndLinks());
            }

            [Theory]
            [AutoData]
            public void GivenSubmitPictureAnswerReturnsSuccessResult_ThenReturnOkRequestResultWithValueAndLinks(
                string username, bool resultData)
            {
                var managerMock = new Mock<IFourWeekChallengeManager>();

                var expectedValue = new Result<bool>(resultData);

                SetupManagerMockSubmitPictureAnswer(username, managerMock, expectedValue);

                new FourWeekChallengeController(managerMock.Object)
                    .SubmitPictureAnswer(username, new Mock<IFormFile>().Object)
                    .Should().BeOkObjectResult()
                    .WithValueEquivalentTo(expectedValue.GetValueAndLinks());
            }

            private static void SetupManagerMockSubmitPictureAnswer(string username,
                Mock<IFourWeekChallengeManager> managerMock, Result<bool> expectedValue)
            {
                managerMock.Setup(manager => manager.SubmitPictureAnswer(username, It.IsAny<IFormFile>()))
                    .Returns(() => expectedValue);
            }
        }

        public class SetUserLocationTests : FourWeekChallengeControllerTests
        {
            [Theory]
            [AutoData]
            public void GivenSetUserLocationReturnsErrorResult_ThenReturnBadRequestResultWithErrorAndLinks(
                string username, decimal latitude, decimal longitude, string errorMessage)
            {
                var managerMock = new Mock<IFourWeekChallengeManager>();

                var expectedValue = new Result(new ErrorMessage(HttpStatusCode.BadRequest, errorMessage));

                SetupManagerMockUpdateUserLocation(username, latitude, longitude, managerMock, expectedValue);

                new FourWeekChallengeController(managerMock.Object)
                    .SetUserLocation(username, new LocationDto(latitude, longitude))
                    .Should().BeBadRequestObjectResult()
                    .WithValueEquivalentTo(expectedValue.GetErrorAndLinks());
            }

            [Theory]
            [AutoData]
            public void GivenSetUserLocationReturnsSuccessResult_ThenReturnNoContent(
                string username, decimal latitude, decimal longitude)
            {
                var managerMock = new Mock<IFourWeekChallengeManager>();

                var expectedValue = new Result().Ok();

                SetupManagerMockUpdateUserLocation(username, latitude, longitude, managerMock, expectedValue);

                new FourWeekChallengeController(managerMock.Object)
                    .SetUserLocation(username, new LocationDto(latitude, longitude))
                    .Should().BeNoContentResult();
            }

            private static void SetupManagerMockUpdateUserLocation(
                string username, decimal latitude, decimal longitude, Mock<IFourWeekChallengeManager> managerMock, Result expectedValue)
            {
                managerMock.Setup(manager => manager.UpdateUserLocation(username, latitude, longitude))
                    .Returns(() => expectedValue);
            }
        }

        public class GetHintTests : FourWeekChallengeControllerTests
        {
            [Theory]
            [AutoData]
            public void GivenGetHintReturnsErrorResult_ThenReturnBadRequestResultWithErrorAndLinks(string username, string errorMessage)
            {
                var managerMock = new Mock<IFourWeekChallengeManager>();

                var expectedValue = new Result<string>(new ErrorMessage(HttpStatusCode.BadRequest, errorMessage));

                SetupManagerMockGetHint(username, managerMock, expectedValue);

                new FourWeekChallengeController(managerMock.Object)
                    .GetHint(username)
                    .Should().BeBadRequestObjectResult()
                    .WithValueEquivalentTo(expectedValue.GetErrorAndLinks());
            }

            [Theory]
            [AutoData]
            public void GivenGetHintReturnsSuccessResult_ThenReturnOkRequestResultWithValueAndLinks(string username, string resultData)
            {
                var managerMock = new Mock<IFourWeekChallengeManager>();

                var expectedValue = new Result<string>(resultData);

                SetupManagerMockGetHint(username, managerMock, expectedValue);

                new FourWeekChallengeController(managerMock.Object)
                    .GetHint(username)
                    .Should().BeOkObjectResult()
                    .WithValueEquivalentTo(expectedValue.GetValueAndLinks());
            }

            private static void SetupManagerMockGetHint(string username, Mock<IFourWeekChallengeManager> managerMock, Result<string> expectedValue)
            {
                managerMock.Setup(manager => manager.GetHintFromQuestion(username))
                    .Returns(() => expectedValue);
            }
        }

        public class GetLocationHintTests : FourWeekChallengeControllerTests
        {
            [Theory]
            [AutoData]
            public void GivenGetLocationHintReturnsErrorResult_ThenReturnBadRequestResultWithErrorAndLinks(string username, string errorMessage)
            {
                var managerMock = new Mock<IFourWeekChallengeManager>();

                var expectedValue = new Result<string>(new ErrorMessage(HttpStatusCode.BadRequest, errorMessage));

                SetupManagerMockGetLocationHint(username, managerMock, expectedValue);

                new FourWeekChallengeController(managerMock.Object)
                    .GetLocationHint(username)
                    .Should().BeBadRequestObjectResult()
                    .WithValueEquivalentTo(expectedValue.GetErrorAndLinks());
            }

            [Theory]
            [AutoData]
            public void GivenGetLocationHintReturnsSuccessResult_ThenReturnOkRequestResultWithValueAndLinks(string username, string resultData)
            {
                var managerMock = new Mock<IFourWeekChallengeManager>();

                var expectedValue = new Result<string>(resultData);

                SetupManagerMockGetLocationHint(username, managerMock, expectedValue);

                new FourWeekChallengeController(managerMock.Object)
                    .GetLocationHint(username)
                    .Should().BeOkObjectResult()
                    .WithValueEquivalentTo(expectedValue.GetValueAndLinks());
            }

            private static void SetupManagerMockGetLocationHint(string username, Mock<IFourWeekChallengeManager> managerMock, Result<string> expectedValue)
            {
                managerMock.Setup(manager => manager.GetLocationHintFromQuestion(username))
                    .Returns(() => expectedValue);
            }
        }

        public class EndGameTests : FourWeekChallengeControllerTests
        {
            [Theory]
            [AutoData]
            public void GivenEndGameReturnsErrorResult_ThenReturnBadRequestResultWithErrorAndLinks(string username, string errorMessage)
            {
                var managerMock = new Mock<IFourWeekChallengeManager>();

                var expectedValue = new Result(new ErrorMessage(HttpStatusCode.BadRequest, errorMessage));

                SetupManagerMockEndGame(username, managerMock, expectedValue);

                new FourWeekChallengeController(managerMock.Object)
                    .EndGame(username)
                    .Should().BeBadRequestObjectResult()
                    .WithValueEquivalentTo(expectedValue.GetErrorAndLinks());
            }

            [Theory]
            [AutoData]
            public void GivenEndGameReturnsSuccessResult_ThenReturnNoContent(string username)
            {
                var managerMock = new Mock<IFourWeekChallengeManager>();

                var expectedValue = new Result().Ok();

                SetupManagerMockEndGame(username, managerMock, expectedValue);

                new FourWeekChallengeController(managerMock.Object)
                    .EndGame(username)
                    .Should().BeOkObjectResult()
                    .WithValueEquivalentTo(new LinkReferencer()
                        .AddLink("categories", FourWeekChallengeEndpoint.GetCategoriesHateoas)
                        .AddLink("score", FourWeekChallengeEndpoint.GetUserScoreRouteWith(username))
                        .AddLink("highScores", FourWeekChallengeEndpoint.GetHighScoresRouteHateoas)
                        .GetLinks());
            }

            private static void SetupManagerMockEndGame(string username, Mock<IFourWeekChallengeManager> managerMock, Result expectedValue)
            {
                managerMock.Setup(manager => manager.EndGame(username))
                    .Returns(() => expectedValue);
            }
        }

        public class GetUserScoreTests : FourWeekChallengeControllerTests
        {
            [Theory]
            [AutoData]
            public void GivenGetUserScoreReturnsErrorResult_ThenReturnBadRequestResultWithErrorAndLinks(string username, string errorMessage)
            {
                var managerMock = new Mock<IFourWeekChallengeManager>();

                var expectedValue = new Result<int>(new ErrorMessage(HttpStatusCode.BadRequest, errorMessage));

                SetupManagerMockGetUserScore(username, managerMock, expectedValue);

                new FourWeekChallengeController(managerMock.Object)
                    .GetUserScore(username)
                    .Should().BeBadRequestObjectResult()
                    .WithValueEquivalentTo(expectedValue.GetErrorAndLinks());
            }

            [Theory]
            [AutoData]
            public void GivenGetUserScoreReturnsSuccessResult_ThenReturnOkRequestResultWithValueAndLinks(string username, int resultData)
            {
                var managerMock = new Mock<IFourWeekChallengeManager>();

                var expectedValue = new Result<int>(resultData);

                SetupManagerMockGetUserScore(username, managerMock, expectedValue);

                new FourWeekChallengeController(managerMock.Object)
                    .GetUserScore(username)
                    .Should().BeOkObjectResult()
                    .WithValueEquivalentTo(expectedValue.GetValueAndLinks());
            }

            private static void SetupManagerMockGetUserScore(string username, Mock<IFourWeekChallengeManager> managerMock, Result<int> expectedValue)
            {
                managerMock.Setup(manager => manager.GetUserScore(username))
                    .Returns(() => expectedValue);
            }
        }

        public class GetHighScoresTests : FourWeekChallengeControllerTests
        {
            [Theory]
            [AutoData]
            public void GivenGetHighScoresReturnsErrorResult_ThenReturnBadRequestResultWithErrorAndLinks(string errorMessage)
            {
                var managerMock = new Mock<IFourWeekChallengeManager>();

                var expectedValue = new Result<List<UserScore>>(new ErrorMessage(HttpStatusCode.BadRequest, errorMessage));

                SetupManagerMockGetHighScores(managerMock, expectedValue);

                new FourWeekChallengeController(managerMock.Object)
                    .GetHighScores()
                    .Should().BeBadRequestObjectResult()
                    .WithValueEquivalentTo(expectedValue.GetErrorAndLinks());
            }

            [Theory]
            [AutoData]
            public void GivenGetHighScoresReturnsSuccessResult_ThenReturnOkRequestResultWithValueAndLinks(List<UserScore> resultData)
            {
                var managerMock = new Mock<IFourWeekChallengeManager>();

                var expectedValue = new Result<List<UserScore>>(resultData);

                SetupManagerMockGetHighScores(managerMock, expectedValue);

                new FourWeekChallengeController(managerMock.Object)
                    .GetHighScores()
                    .Should().BeOkObjectResult()
                    .WithValueEquivalentTo(expectedValue.GetValueAndLinks());
            }

            private static void SetupManagerMockGetHighScores(Mock<IFourWeekChallengeManager> managerMock, Result<List<UserScore>> expectedValue)
            {
                managerMock.Setup(manager => manager.GetHighScores())
                    .Returns(() => expectedValue);
            }
        }

        public void Dispose()
        {
            Context.Dispose();
        }
    }
}