using Assignment4WC.Context.Models;
using Assignment4WC.Context.Repositories;
using Assignment4WC.Logic;
using Assignment4WC.Models;
using Assignment4WC.Models.ControllerEndpoints;
using Assignment4WC.Models.ResultType;
using AutoFixture;
using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Xunit;

namespace Assignment4WC.Tests.Logic
{
    public class FourWeekChallengeManagerTests
    {
        protected readonly IFixture Fixture;
        protected FourWeekChallengeManagerTests()
        {
            Fixture = new Fixture();
            Fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
            Fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        public class GetCategoriesAndQuestionCountTests : FourWeekChallengeManagerTests
        {
            [Theory]
            [AutoData]
            public void ThenReturnsCategoriesWithQuestionCount(int questionCount)
            {
                var repositoryMock = new Mock<IGlobalRepository>(MockBehavior.Strict);

                SetupMockCountQuestionsFromCategory(repositoryMock, questionCount);

                var expectedValue = new Result<List<CategoryWithQuestionCount>>(
                    Enum.GetNames(typeof(CategoryType))
                        .Select(categoryString => (CategoryType) Enum.Parse(typeof(CategoryType), categoryString))
                        .Select(category => new CategoryWithQuestionCount(
                            category.ToString(),
                            GetQuestionCountInIncrements(questionCount, 5)))
                        .ToList()).AddLink(FourWeekChallengeEndpoint.StartRoute);

                new FourWeekChallengeManager(repositoryMock.Object, new Mock<IQuestionRandomiser>().Object)
                    .GetCategoriesAndQuestionCount()
                    .Should().BeResultEquivalentTo(expectedValue);
            }

            private static void SetupMockCountQuestionsFromCategory(Mock<IGlobalRepository> repositoryMock,
                int questionCount)
            {
                repositoryMock.Setup(repository =>
                        repository.Questions.CountQuestionsFromCategory(It.IsAny<CategoryType>()))
                    .Returns(questionCount);
            }

            private IEnumerable<int> GetQuestionCountInIncrements(int questionCount, int increments)
            {
                for (var i = 0; i < questionCount / increments; i++)
                {
                    yield return increments * (i + 1);
                }
            }
        }

        public class UpdateUserLocationTests : FourWeekChallengeManagerTests
        {
            [Theory]
            [AutoData]
            public void GivenUsernameDoesNotExist_ThenReturnMemberDoesNotExistErrorResult(string username,
                decimal latitude, decimal longitude)
            {
                var repositoryMock = new Mock<IGlobalRepository>(MockBehavior.Strict);

                SetupMockGetMemberOrNull(repositoryMock, username, null);

                var expectedValue = new Result(new ErrorMessage(HttpStatusCode.NotFound,
                    $"Member with username '{username}' does not exist."))
                    .AddLink(FourWeekChallengeEndpoint.GetCategories);

                new FourWeekChallengeManager(repositoryMock.Object, new Mock<IQuestionRandomiser>().Object)
                    .UpdateUserLocation(username, latitude, longitude)
                    .Should().BeResultEquivalentTo(expectedValue);
            }

            [Theory]
            [AutoData]
            public void GivenMemberDoesNotHaveAnInitialLocationSet_ThenSetMemberLocationAndReturnSuccessResult(
                int appId, string username, decimal latitude, decimal longitude, int locationId)
            {
                var repositoryMock = new Mock<IGlobalRepository>(MockBehavior.Strict);

                var fakeMember = new Members()
                {
                    AppId = appId,
                    Username = username,
                };

                var newLocationWithId = Fixture.Build<Locations>()
                    .With(locations => locations.LocationId, locationId)
                    .Create();

                SetupMockGetMemberOrNull(repositoryMock, username, fakeMember);
                SetUpMockAddLocation(repositoryMock);
                SetupMockGetLocationByLocation(repositoryMock, newLocationWithId);
                SetupMockSaveChanges(repositoryMock);

                var expectedValue = new Result().Ok();

                new FourWeekChallengeManager(repositoryMock.Object, new Mock<IQuestionRandomiser>().Object)
                    .UpdateUserLocation(username, latitude, longitude)
                    .Should().BeResultEquivalentTo(expectedValue);

                repositoryMock.Verify(repository => repository.SaveChanges(), Times.Exactly(2));
            }

            [Theory]
            [AutoData]
            public void GivenMemberHasAnInitialLocationSet_ThenUpdateMemberLocationAndReturnSuccessResult(
                int appId, string username, decimal latitude, decimal longitude, int locationId)
            {
                var repositoryMock = new Mock<IGlobalRepository>(MockBehavior.Strict);

                var fakeMember = new Members()
                {
                    AppId = appId,
                    Username = username,
                    LocationId = locationId
                };

                var newLocationWithId = Fixture.Build<Locations>()
                    .With(locations => locations.LocationId, locationId)
                    .Create();

                SetupMockGetMemberOrNull(repositoryMock, username, fakeMember);
                SetUpMockGetLocationByLocationId(repositoryMock, locationId, newLocationWithId);
                SetupMockSaveChanges(repositoryMock);

                var expectedValue = new Result().Ok();

                new FourWeekChallengeManager(repositoryMock.Object, new Mock<IQuestionRandomiser>().Object)
                    .UpdateUserLocation(username, latitude, longitude)
                    .Should().BeResultEquivalentTo(expectedValue);

                repositoryMock.Verify(repository => repository.SaveChanges(), Times.Once);
            }

            private static void SetupMockGetLocationByLocation(Mock<IGlobalRepository> repositoryMock, Locations newLocationWithId)
            {
                repositoryMock.Setup(repository => repository.Locations.GetLocationByLocation(
                        It.IsAny<Locations>()))
                    .Returns(newLocationWithId);
            }

            private static void SetUpMockGetLocationByLocationId(Mock<IGlobalRepository> repositoryMock, int locationId,
                Locations newLocationWithId)
            {
                repositoryMock.Setup(repository => repository.Locations.GetLocationByLocationId(
                        locationId))
                    .Returns(newLocationWithId);
            }


            private static void SetUpMockAddLocation(Mock<IGlobalRepository> repositoryMock)
            {
                repositoryMock.Setup(repository => repository.Locations.Add(It.IsAny<Locations>()));
            }
        }

        public class GetHintFromQuestionTests : FourWeekChallengeManagerTests
        {
            [Theory]
            [AutoData]
            public void GivenUsernameDoesNotExist_ThenReturnMemberDoesNotExistErrorResult(string username)
            {
                var repositoryMock = new Mock<IGlobalRepository>(MockBehavior.Strict);

                SetupMockGetMemberOrNull(repositoryMock, username, null);

                var expectedValue = new Result(new ErrorMessage(HttpStatusCode.NotFound,
                        $"Member with username '{username}' does not exist."))
                    .AddLink(FourWeekChallengeEndpoint.GetCategories);

                new FourWeekChallengeManager(repositoryMock.Object, new Mock<IQuestionRandomiser>().Object)
                    .GetHintFromQuestion(username)
                    .Should().BeResultEquivalentTo(expectedValue);
            }

            [Fact]
            public void GivenCurrentQuestionNumberIsOutsideTheRangeOfIndexesInQuestionIds_ThenReturnIndexOutOfRangeErrorResult()
            {
                var repositoryMock = new Mock<IGlobalRepository>(MockBehavior.Strict);

                var fakeMember = Fixture.Build<Members>()
                    .With(members => members.QuestionIds, () => BuildQuestionIds(10))
                    .Create();

                fakeMember.CurrentQuestionNumber = fakeMember.QuestionIds.Split(",").Length + 1;

                SetupMockGetMemberOrNull(repositoryMock, fakeMember.Username, fakeMember);

                var expectedValue = new Result<string>(new ErrorMessage(HttpStatusCode.InternalServerError,
                    $"Index '{nameof(fakeMember.CurrentQuestionNumber)}' was outside the range for the number of questionIds the member has."));

                new FourWeekChallengeManager(repositoryMock.Object, new Mock<IQuestionRandomiser>().Object)
                    .GetHintFromQuestion(fakeMember.Username)
                    .Should().BeResultEquivalentTo(expectedValue);
            }

            [Fact]
            public void GivenCurrentQuestionNumberIsEqualToQuestionIdsLength_ThenReturnGameHasEndedErrorResult()
            {
                var repositoryMock = new Mock<IGlobalRepository>(MockBehavior.Strict);

                var fakeMember = Fixture.Build<Members>()
                    .With(members => members.QuestionIds, BuildQuestionIds(10))
                    .Create();

                fakeMember.CurrentQuestionNumber = fakeMember.QuestionIds.Split(",").Length;

                SetupMockGetMemberOrNull(repositoryMock, fakeMember.Username, fakeMember);

                var expectedValue = new Result<string>(new ErrorMessage(HttpStatusCode.NotFound, $"Game has ended for member with name '{fakeMember.Username}'."))
                    .AddLink(FourWeekChallengeEndpoint.EndGameRouteWith(fakeMember.Username));

                new FourWeekChallengeManager(repositoryMock.Object, new Mock<IQuestionRandomiser>().Object)
                    .GetHintFromQuestion(fakeMember.Username)
                    .Should().BeResultEquivalentTo(expectedValue);
            }

            [Fact]
            public void GivenComplexQuestionDataIsNullAndQuestionWithQuestionIdDoesNotExist_ThenReturnQuestionDoesNotExistErrorResult()
            {
                var repositoryMock = new Mock<IGlobalRepository>(MockBehavior.Strict);

                var questionIds = BuildQuestionIds(10);

                var fakeMember = Fixture.Build<Members>()
                    .With(members => members.QuestionIds, questionIds)
                    .With(members => members.CurrentQuestionNumber, 0)
                    .Create();

                var currentQuestionId = int.Parse(questionIds.Split(",")[fakeMember.CurrentQuestionNumber]);

                SetupMockGetMemberOrNull(repositoryMock, fakeMember.Username, fakeMember);
                SetupMockGetComplexQuestion(repositoryMock, currentQuestionId, null);
                SetupMockDoesQuestionExist(repositoryMock, currentQuestionId, false);

                var expectedValue = new Result<string>(new ErrorMessage(HttpStatusCode.NotFound,
                        $"Question with ID '{currentQuestionId}' does not exist in database."))
                    .AddLink(FourWeekChallengeEndpoint.GetQuestionRouteWith(fakeMember.Username));

                new FourWeekChallengeManager(repositoryMock.Object, new Mock<IQuestionRandomiser>().Object)
                    .GetHintFromQuestion(fakeMember.Username)
                    .Should().BeResultEquivalentTo(expectedValue);
            }

            [Fact]
            public void GivenComplexQuestionDataIsNullAndQuestionWithQuestionIdDoesExist_ThenReturnThisQuestionIsNotComplexErrorResult()
            {
                var repositoryMock = new Mock<IGlobalRepository>(MockBehavior.Strict);

                var questionIds = BuildQuestionIds(10);

                var fakeMember = Fixture.Build<Members>()
                    .With(members => members.QuestionIds, questionIds)
                    .With(members => members.CurrentQuestionNumber, 0)
                    .Create();

                var currentQuestionId = int.Parse(questionIds.Split(",")[fakeMember.CurrentQuestionNumber]);

                SetupMockGetMemberOrNull(repositoryMock, fakeMember.Username, fakeMember);
                SetupMockGetComplexQuestion(repositoryMock, currentQuestionId, null);
                SetupMockDoesQuestionExist(repositoryMock, currentQuestionId, true);

                var expectedValue = new Result<string>(new ErrorMessage(HttpStatusCode.BadRequest,
                        $"This question is not a complex question. Cannot provide additional details for this question."))
                    .AddLink(FourWeekChallengeEndpoint.GetQuestionRouteWith(fakeMember.Username));

                new FourWeekChallengeManager(repositoryMock.Object, new Mock<IQuestionRandomiser>().Object)
                    .GetHintFromQuestion(fakeMember.Username)
                    .Should().BeResultEquivalentTo(expectedValue);
            }

            [Theory]
            [AutoData]
            public void GivenComplexQuestionData_ThenReturnComplexQuestionHintInSuccessResult(bool questionExists)
            {
                var repositoryMock = new Mock<IGlobalRepository>(MockBehavior.Strict);

                var questionIds = BuildQuestionIds(10);

                var fakeMember = Fixture.Build<Members>()
                    .With(members => members.QuestionIds, questionIds)
                    .With(members => members.CurrentQuestionNumber, 0)
                    .Create();

                var currentQuestionId = int.Parse(questionIds.Split(",")[fakeMember.CurrentQuestionNumber]);

                var fakeComplexQuestion = Fixture.Create<ComplexQuestions>();

                SetupMockGetMemberOrNull(repositoryMock, fakeMember.Username, fakeMember);
                SetupMockGetComplexQuestion(repositoryMock, currentQuestionId, fakeComplexQuestion);
                SetupMockDoesQuestionExist(repositoryMock, currentQuestionId, questionExists);
                SetupMockSaveChanges(repositoryMock);

                var expectedValue = new Result<string>(fakeComplexQuestion.Hint);

                new FourWeekChallengeManager(repositoryMock.Object, new Mock<IQuestionRandomiser>().Object)
                    .GetHintFromQuestion(fakeMember.Username)
                    .Should().BeResultEquivalentTo(expectedValue);

                repositoryMock.Verify(repository => repository.SaveChanges(), Times.Once);
            }
        }

        public class AddNewPlayerTests : FourWeekChallengeManagerTests
        {
            [Theory]
            [AutoData]
            public void GivenUsernameAlreadyExists_ThenReturnMemberWithUsernameExistsErrorResult(
                int appId, string username, CategoryType category, int numOfQuestions)
            {
                var repositoryMock = new Mock<IGlobalRepository>(MockBehavior.Strict);

                SetupMockDoesUsernameExist(repositoryMock, username, true);

                var expectedValue = new Result(new ErrorMessage(HttpStatusCode.BadRequest,
                        $"The username '{username}' already exists, try another."))
                    .AddLink(FourWeekChallengeEndpoint.StartRouteWith(appId.ToString()));

                new FourWeekChallengeManager(repositoryMock.Object, new Mock<IQuestionRandomiser>().Object)
                    .AddNewPlayer(appId, username, category, numOfQuestions)
                    .Should().BeResultEquivalentTo(expectedValue);
            }

            [Theory]
            [AutoData]
            public void GivenCategoryDoesNotExist_ThenReturnCategoryDoesNotExistErrorResult(
                int appId, string username, int numOfQuestions)
            {
                var repositoryMock = new Mock<IGlobalRepository>(MockBehavior.Strict);

                var category = (CategoryType)int.MaxValue;

                var expectedValue = new Result(new ErrorMessage(HttpStatusCode.NotFound,
                        $"Category '{category}' does not exist in the database."))
                    .AddLink(FourWeekChallengeEndpoint.StartRouteWith(appId.ToString()));

                SetupMockDoesUsernameExist(repositoryMock, username, false);
                SetupMockGetCategoryIdFromCategory(repositoryMock, category, null);

                new FourWeekChallengeManager(repositoryMock.Object, new QuestionRandomiser(repositoryMock.Object))
                    .AddNewPlayer(appId, username, category, numOfQuestions)
                    .Should().BeResultEquivalentTo(expectedValue);
            }

            [Theory]
            [AutoData]
            public void GivenNumOfQuestionsRequestedIsLargerThanActualNumOfQuestions_ThenReturnCategoryDoesNotExistErrorResult(
                int appId, string username, CategoryType category, int categoryId, List<int> allQuestionIdsForCategory)
            {
                var repositoryMock = new Mock<IGlobalRepository>(MockBehavior.Strict);

                var requestedNumOfQuestions = allQuestionIdsForCategory.Count + 1;

                var expectedValue = new Result(new ErrorMessage(HttpStatusCode.BadRequest,
                    $"{requestedNumOfQuestions} question(s) from category '{category}' were requested but this category only contains {allQuestionIdsForCategory.Count} question(s)."))
                    .AddLink(FourWeekChallengeEndpoint.StartRouteWith(appId.ToString()));

                SetupMockDoesUsernameExist(repositoryMock, username, false);
                SetupMockGetCategoryIdFromCategory(repositoryMock, category, categoryId);
                SetupMockGetAllQuestionsByCategoryId(repositoryMock, categoryId, allQuestionIdsForCategory);

                new FourWeekChallengeManager(repositoryMock.Object, new QuestionRandomiser(repositoryMock.Object))
                    .AddNewPlayer(appId, username, category, requestedNumOfQuestions)
                    .Should().BeResultEquivalentTo(expectedValue);
            }

            [Theory]
            [AutoData]
            public void GivenRandomisedQuestionIds_ThenReturnSuccessResult(
                int appId, string username, CategoryType category, int categoryId, List<int> allQuestionIdsForCategory)
            {
                var repositoryMock = new Mock<IGlobalRepository>(MockBehavior.Strict);

                var requestedNumOfQuestions = allQuestionIdsForCategory.Count;

                var expectedValue = new Result().Ok();

                SetupMockDoesUsernameExist(repositoryMock, username, false);
                SetupMockGetCategoryIdFromCategory(repositoryMock, category, categoryId);
                SetupMockGetAllQuestionsByCategoryId(repositoryMock, categoryId, allQuestionIdsForCategory);
                SetupMockAddMember(repositoryMock);
                SetupMockSaveChanges(repositoryMock);

                new FourWeekChallengeManager(repositoryMock.Object, new QuestionRandomiser(repositoryMock.Object))
                    .AddNewPlayer(appId, username, category, requestedNumOfQuestions)
                    .Should().BeResultEquivalentTo(expectedValue);

                repositoryMock.Verify(repository => repository.Members.Add(It.IsAny<Members>()), Times.Once);
                repositoryMock.Verify(repository => repository.SaveChanges(), Times.Once);
            }

            private static void SetupMockAddMember(Mock<IGlobalRepository> repositoryMock)
            {
                repositoryMock.Setup(repository => repository.Members.Add(It.IsAny<Members>()));
            }

            private static void SetupMockGetAllQuestionsByCategoryId(Mock<IGlobalRepository> repositoryMock,
                int categoryId,
                List<int> allQuestionIdsForCategory)
            {
                repositoryMock.Setup(repository => repository.Questions.GetAllQuestionsByCategoryId(categoryId))
                    .Returns(allQuestionIdsForCategory);
            }

            private static void SetupMockGetCategoryIdFromCategory(Mock<IGlobalRepository> repositoryMock,
                CategoryType category, int? expected)
            {
                repositoryMock.Setup(repository => repository.Categories.GetCategoryIdFromCategory(category))
                    .Returns(() => expected);
            }

            private static void SetupMockDoesUsernameExist(Mock<IGlobalRepository> repositoryMock, string username,
                bool expected)
            {
                repositoryMock.Setup(repository => repository.Members.DoesUsernameExist(username))
                    .Returns(expected);
            }
        }

        public class GetCurrentQuestionDataTests : FourWeekChallengeManagerTests
        {
            [Theory]
            [AutoData]
            public void GivenUsernameDoesNotExist_ThenReturnMemberDoesNotExistErrorResult(string username)
            {
                var repositoryMock = new Mock<IGlobalRepository>(MockBehavior.Strict);

                SetupMockGetMemberOrNull(repositoryMock, username, null);

                var expectedValue = new Result(new ErrorMessage(HttpStatusCode.NotFound,
                        $"Member with username '{username}' does not exist."))
                    .AddLink(FourWeekChallengeEndpoint.GetCategories);

                new FourWeekChallengeManager(repositoryMock.Object, new Mock<IQuestionRandomiser>().Object)
                    .GetCurrentQuestionData(username)
                    .Should().BeResultEquivalentTo(expectedValue);
            }

            [Fact]
            public void GivenCurrentQuestionNumberIsOutsideTheRangeOfIndexesInQuestionIds_ThenReturnIndexOutOfRangeErrorResult()
            {
                var repositoryMock = new Mock<IGlobalRepository>(MockBehavior.Strict);

                var fakeMember = Fixture.Build<Members>()
                    .With(members => members.QuestionIds, () => BuildQuestionIds(10))
                    .Create();
                ;
                fakeMember.CurrentQuestionNumber = fakeMember.QuestionIds.Split(",").Length + 1;

                SetupMockGetMemberOrNull(repositoryMock, fakeMember.Username, fakeMember);

                var expectedValue = new Result<Questions>(new ErrorMessage(HttpStatusCode.InternalServerError,
                    $"Index '{nameof(fakeMember.CurrentQuestionNumber)}' was outside the range for the number of questionIds the member has."));

                new FourWeekChallengeManager(repositoryMock.Object, new Mock<IQuestionRandomiser>().Object)
                    .GetCurrentQuestionData(fakeMember.Username)
                    .Should().BeResultEquivalentTo(expectedValue);
            }

            [Fact]
            public void GivenCurrentQuestionNumberIsEqualToQuestionIdsLength_ThenReturnGameHasEndedErrorResult()
            {
                var repositoryMock = new Mock<IGlobalRepository>(MockBehavior.Strict);

                var fakeMember = Fixture.Build<Members>()
                    .With(members => members.QuestionIds, BuildQuestionIds(10))
                    .Create();

                fakeMember.CurrentQuestionNumber = fakeMember.QuestionIds.Split(",").Length;

                SetupMockGetMemberOrNull(repositoryMock, fakeMember.Username, fakeMember);

                var expectedValue = new Result<Questions>(new ErrorMessage(HttpStatusCode.NotFound, $"Game has ended for member with name '{fakeMember.Username}'."))
                    .AddLink(FourWeekChallengeEndpoint.EndGameRouteWith(fakeMember.Username));

                new FourWeekChallengeManager(repositoryMock.Object, new Mock<IQuestionRandomiser>().Object)
                    .GetCurrentQuestionData(fakeMember.Username)
                    .Should().BeResultEquivalentTo(expectedValue);
            }

            [Fact]
            public void GivenQuestionIdDoesNotMatchAQuestion_ThenReturnQuestionWithQuestionIdDoesNotExistErrorResult()
            {
                var repositoryMock = new Mock<IGlobalRepository>(MockBehavior.Strict);

                var questionIds = BuildQuestionIds(10);

                var fakeMember = Fixture.Build<Members>()
                    .With(members => members.QuestionIds, questionIds)
                    .With(members => members.CurrentQuestionNumber, 0)
                    .Create();

                var currentQuestionId = int.Parse(questionIds.Split(",")[fakeMember.CurrentQuestionNumber]);

                var expectedValue = new Result<Questions>(
                        new ErrorMessage(
                            HttpStatusCode.NotFound,
                            $"Question with ID '{currentQuestionId}' does not exist in database."))
                    .AddLink(FourWeekChallengeEndpoint.GetQuestionRouteWith(fakeMember.Username));

                SetupMockGetMemberOrNull(repositoryMock, fakeMember.Username, fakeMember);
                SetupMockGetQuestionOrNull(repositoryMock, currentQuestionId, null);

                new FourWeekChallengeManager(repositoryMock.Object, new Mock<IQuestionRandomiser>().Object)
                    .GetCurrentQuestionData(fakeMember.Username)
                    .Should().BeResultEquivalentTo(expectedValue);
            }

            [Fact]
            public void GivenQuestionComplexityIsSimple_ThenReturnQuestionSuccessResult()
            {
                var repositoryMock = new Mock<IGlobalRepository>(MockBehavior.Strict);

                var questionIds = BuildQuestionIds(10);

                var fakeMember = Fixture.Build<Members>()
                    .With(members => members.QuestionIds, questionIds)
                    .With(members => members.CurrentQuestionNumber, 0)
                    .Create();

                var currentQuestionId = int.Parse(questionIds.Split(",")[fakeMember.CurrentQuestionNumber]);

                var question = Fixture.Build<Questions>()
                    .With(question => question.Discriminator, QuestionComplexity.Simple.ToString)
                    .Create();

                var expectedValue = new Result<Questions>(question);

                SetupMockGetMemberOrNull(repositoryMock, fakeMember.Username, fakeMember);
                SetupMockGetQuestionOrNull(repositoryMock, currentQuestionId, question);

                new FourWeekChallengeManager(repositoryMock.Object, new Mock<IQuestionRandomiser>().Object)
                    .GetCurrentQuestionData(fakeMember.Username)
                    .Should().BeResultEquivalentTo(expectedValue);
            }

            [Fact]
            public void GivenQuestionComplexityIsComplex_ThenReturnQuestionSuccessResultWithLinks()
            {
                var repositoryMock = new Mock<IGlobalRepository>(MockBehavior.Strict);

                var questionIds = BuildQuestionIds(10);

                var fakeMember = Fixture.Build<Members>()
                    .With(members => members.QuestionIds, questionIds)
                    .With(members => members.CurrentQuestionNumber, 0)
                    .Create();

                var currentQuestionId = int.Parse(questionIds.Split(",")[fakeMember.CurrentQuestionNumber]);

                var question = Fixture.Build<Questions>()
                    .With(question => question.Discriminator, QuestionComplexity.Complex.ToString)
                    .Create();

                var expectedValue = new Result<Questions>(question)
                    .AddLink("hint", FourWeekChallengeEndpoint.GetHintRouteWith(fakeMember.Username))
                    .AddLink("setLocation", FourWeekChallengeEndpoint.SetUserLocationRouteWith(fakeMember.Username))
                    .AddLink("locationHint", FourWeekChallengeEndpoint.GetLocationHintRouteWith(fakeMember.Username));

                SetupMockGetMemberOrNull(repositoryMock, fakeMember.Username, fakeMember);
                SetupMockGetQuestionOrNull(repositoryMock, currentQuestionId, question);

                new FourWeekChallengeManager(repositoryMock.Object, new Mock<IQuestionRandomiser>().Object)
                    .GetCurrentQuestionData(fakeMember.Username)
                    .Should().BeResultEquivalentTo(expectedValue);
            }
        }

        public class GetLocationHintFromQuestionTests : FourWeekChallengeManagerTests
        {
            [Theory]
            [AutoData]
            public void GivenUsernameDoesNotExist_ThenReturnMemberDoesNotExistErrorResult(string username)
            {
                var repositoryMock = new Mock<IGlobalRepository>(MockBehavior.Strict);

                SetupMockGetMemberOrNull(repositoryMock, username, null);

                var expectedValue = new Result(new ErrorMessage(HttpStatusCode.NotFound,
                        $"Member with username '{username}' does not exist."))
                    .AddLink(FourWeekChallengeEndpoint.GetCategories);

                new FourWeekChallengeManager(repositoryMock.Object, new Mock<IQuestionRandomiser>().Object)
                    .GetLocationHintFromQuestion(username)
                    .Should().BeResultEquivalentTo(expectedValue);
            }

            [Fact]
            public void GivenCurrentQuestionNumberIsOutsideTheRangeOfIndexesInQuestionIds_ThenReturnIndexOutOfRangeErrorResult()
            {
                var repositoryMock = new Mock<IGlobalRepository>(MockBehavior.Strict);

                var fakeMember = Fixture.Build<Members>()
                    .With(members => members.QuestionIds, () => BuildQuestionIds(10))
                    .Create();

                fakeMember.CurrentQuestionNumber = fakeMember.QuestionIds.Split(",").Length + 1;

                SetupMockGetMemberOrNull(repositoryMock, fakeMember.Username, fakeMember);

                var expectedValue = new Result<string>(new ErrorMessage(HttpStatusCode.InternalServerError,
                    $"Index '{nameof(fakeMember.CurrentQuestionNumber)}' was outside the range for the number of questionIds the member has."));

                new FourWeekChallengeManager(repositoryMock.Object, new Mock<IQuestionRandomiser>().Object)
                    .GetLocationHintFromQuestion(fakeMember.Username)
                    .Should().BeResultEquivalentTo(expectedValue);
            }

            [Fact]
            public void GivenCurrentQuestionNumberIsEqualToQuestionIdsLength_ThenReturnGameHasEndedErrorResult()
            {
                var repositoryMock = new Mock<IGlobalRepository>(MockBehavior.Strict);

                var fakeMember = Fixture.Build<Members>()
                    .With(members => members.QuestionIds, BuildQuestionIds(10))
                    .Create();

                fakeMember.CurrentQuestionNumber = fakeMember.QuestionIds.Split(",").Length;

                SetupMockGetMemberOrNull(repositoryMock, fakeMember.Username, fakeMember);

                var expectedValue = new Result<string>(new ErrorMessage(HttpStatusCode.NotFound, $"Game has ended for member with name '{fakeMember.Username}'."))
                    .AddLink(FourWeekChallengeEndpoint.EndGameRouteWith(fakeMember.Username));

                new FourWeekChallengeManager(repositoryMock.Object, new Mock<IQuestionRandomiser>().Object)
                    .GetLocationHintFromQuestion(fakeMember.Username)
                    .Should().BeResultEquivalentTo(expectedValue);
            }

            [Fact]
            public void GivenComplexQuestionDataIsNullAndQuestionWithQuestionIdDoesNotExist_ThenReturnQuestionDoesNotExistErrorResult()
            {
                var repositoryMock = new Mock<IGlobalRepository>(MockBehavior.Strict);

                var questionIds = BuildQuestionIds(10);

                var fakeMember = Fixture.Build<Members>()
                    .With(members => members.QuestionIds, questionIds)
                    .With(members => members.CurrentQuestionNumber, 0)
                    .Create();

                var currentQuestionId = int.Parse(questionIds.Split(",")[fakeMember.CurrentQuestionNumber]);

                SetupMockGetMemberOrNull(repositoryMock, fakeMember.Username, fakeMember);
                SetupMockGetComplexQuestion(repositoryMock, currentQuestionId, null);
                SetupMockDoesQuestionExist(repositoryMock, currentQuestionId, false);

                var expectedValue = new Result<string>(new ErrorMessage(HttpStatusCode.NotFound,
                        $"Question with ID '{currentQuestionId}' does not exist in database."))
                    .AddLink(FourWeekChallengeEndpoint.GetQuestionRouteWith(fakeMember.Username));

                new FourWeekChallengeManager(repositoryMock.Object, new Mock<IQuestionRandomiser>().Object)
                    .GetLocationHintFromQuestion(fakeMember.Username)
                    .Should().BeResultEquivalentTo(expectedValue);
            }

            [Fact]
            public void GivenComplexQuestionDataIsNullAndQuestionWithQuestionIdDoesExist_ThenReturnThisQuestionIsNotComplexErrorResult()
            {
                var repositoryMock = new Mock<IGlobalRepository>(MockBehavior.Strict);

                var questionIds = BuildQuestionIds(10);

                var fakeMember = Fixture.Build<Members>()
                    .With(members => members.QuestionIds, questionIds)
                    .With(members => members.CurrentQuestionNumber, 0)
                    .Create();

                var currentQuestionId = int.Parse(questionIds.Split(",")[fakeMember.CurrentQuestionNumber]);

                SetupMockGetMemberOrNull(repositoryMock, fakeMember.Username, fakeMember);
                SetupMockGetComplexQuestion(repositoryMock, currentQuestionId, null);
                SetupMockDoesQuestionExist(repositoryMock, currentQuestionId, true);

                var expectedValue = new Result<string>(new ErrorMessage(HttpStatusCode.BadRequest,
                        $"This question is not a complex question. Cannot provide additional details for this question."))
                    .AddLink(FourWeekChallengeEndpoint.GetQuestionRouteWith(fakeMember.Username));

                new FourWeekChallengeManager(repositoryMock.Object, new Mock<IQuestionRandomiser>().Object)
                    .GetLocationHintFromQuestion(fakeMember.Username)
                    .Should().BeResultEquivalentTo(expectedValue);
            }

            [Theory]
            [AutoData]
            public void GivenComplexQuestionData_ThenReturnComplexQuestionHintInSuccessResult(bool questionExists)
            {
                var repositoryMock = new Mock<IGlobalRepository>(MockBehavior.Strict);

                var questionIds = BuildQuestionIds(10);

                var fakeMember = Fixture.Build<Members>()
                    .With(members => members.QuestionIds, questionIds)
                    .With(members => members.CurrentQuestionNumber, 0)
                    .Create();

                var currentQuestionId = int.Parse(questionIds.Split(",")[fakeMember.CurrentQuestionNumber]);

                var fakeComplexQuestion = Fixture.Create<ComplexQuestions>();

                SetupMockGetMemberOrNull(repositoryMock, fakeMember.Username, fakeMember);
                SetupMockGetComplexQuestion(repositoryMock, currentQuestionId, fakeComplexQuestion);
                SetupMockDoesQuestionExist(repositoryMock, currentQuestionId, questionExists);
                SetupMockSaveChanges(repositoryMock);

                var expectedValue = new Result<string>(fakeComplexQuestion.LocationHint);

                new FourWeekChallengeManager(repositoryMock.Object, new Mock<IQuestionRandomiser>().Object)
                    .GetLocationHintFromQuestion(fakeMember.Username)
                    .Should().BeResultEquivalentTo(expectedValue);

                repositoryMock.Verify(repository => repository.SaveChanges(), Times.Once);
            }
        }

        public class EndGameTests : FourWeekChallengeManagerTests
        {
            [Theory]
            [AutoData]
            public void GivenUsernameDoesNotExist_ThenReturnMemberDoesNotExistErrorResult(string username)
            {
                var repositoryMock = new Mock<IGlobalRepository>(MockBehavior.Strict);

                SetupMockGetMemberOrNull(repositoryMock, username, null);

                var expectedValue = new Result(new ErrorMessage(HttpStatusCode.NotFound,
                        $"Member with username '{username}' does not exist."))
                    .AddLink(FourWeekChallengeEndpoint.GetCategories);

                new FourWeekChallengeManager(repositoryMock.Object, new Mock<IQuestionRandomiser>().Object)
                    .EndGame(username)
                    .Should().BeResultEquivalentTo(expectedValue);
            }

            [Fact]
            public void GivenGameHasNotEnded_ThenReturnGameHasNotEnededErrorResult()
            {
                var repositoryMock = new Mock<IGlobalRepository>(MockBehavior.Strict);

                var fakeMember = Fixture.Build<Members>()
                    .With(members => members.QuestionIds, BuildQuestionIds(10))
                    .With(members => members.CurrentQuestionNumber, 0)
                    .Create();

                SetupMockGetMemberOrNull(repositoryMock, fakeMember.Username, fakeMember);

                var expectedValue = new Result(new ErrorMessage(HttpStatusCode.BadRequest,
                        $"Game has not ended for user with name '{fakeMember.Username}.'"))
                    .AddLink(FourWeekChallengeEndpoint.GetQuestionRouteWith(fakeMember.Username));

                new FourWeekChallengeManager(repositoryMock.Object, new Mock<IQuestionRandomiser>().Object)
                    .EndGame(fakeMember.Username)
                    .Should().BeResultEquivalentTo(expectedValue);
            }

            [Fact]
            public void GivenGameHaEnded_ThenReturnSuccessResult()
            {
                var repositoryMock = new Mock<IGlobalRepository>(MockBehavior.Strict);

                var fakeMember = Fixture.Build<Members>()
                    .With(members => members.QuestionIds, BuildQuestionIds(10))
                    .With(members => members.CurrentQuestionNumber, 10)
                    .Create();

                SetupMockGetMemberOrNull(repositoryMock, fakeMember.Username, fakeMember);

                var expectedValue = new Result().Ok();

                new FourWeekChallengeManager(repositoryMock.Object, new Mock<IQuestionRandomiser>().Object)
                    .EndGame(fakeMember.Username)
                    .Should().BeResultEquivalentTo(expectedValue);
            }
        }

        public class GetUserScoreTests : FourWeekChallengeManagerTests
        {
            [Theory]
            [AutoData]
            public void GivenUsernameDoesNotExist_ThenReturnMemberDoesNotExistErrorResult(string username)
            {
                var repositoryMock = new Mock<IGlobalRepository>(MockBehavior.Strict);

                SetupMockGetMemberOrNull(repositoryMock, username, It.IsAny<Members>());

                var expectedValue = new Result<int>(new ErrorMessage(HttpStatusCode.NotFound,
                        $"Member with username '{username}' does not exist."))
                    .AddLink(FourWeekChallengeEndpoint.GetCategories);

                new FourWeekChallengeManager(repositoryMock.Object, new Mock<IQuestionRandomiser>().Object)
                    .GetUserScore(username)
                    .Should().BeResultEquivalentTo(expectedValue);
            }

            [Fact]
            public void GivenMemberExists_ThenReturnUserScoreInSuccessResult()
            {
                var repositoryMock = new Mock<IGlobalRepository>(MockBehavior.Strict);

                var fakeMember = Fixture.Create<Members>();

                SetupMockGetMemberOrNull(repositoryMock, fakeMember.Username, fakeMember);

                var expectedValue = new Result<int>(fakeMember.UserScore)
                    .AddLink("category", FourWeekChallengeEndpoint.GetCategories)
                    .AddLink("highScore", FourWeekChallengeEndpoint.GetHighScoresRoute);

                new FourWeekChallengeManager(repositoryMock.Object, new Mock<IQuestionRandomiser>().Object)
                    .GetUserScore(fakeMember.Username)
                    .Should().BeResultEquivalentTo(expectedValue);
            }
        }

        public class GetHighScoresTests : FourWeekChallengeManagerTests
        {
            [Fact]
            public void GivenThereAreNoMembersThatCurrentlyExist_ThenReturnNoMembersExistErrorResult()
            {
                var repositoryMock = new Mock<IGlobalRepository>(MockBehavior.Strict);

                SetupMockAnyMembersExist(repositoryMock, false);

                var expectedValue = new Result<List<UserScore>>(
                        new ErrorMessage(HttpStatusCode.BadRequest, "No members currently exist."))
                    .AddLink("categories", FourWeekChallengeEndpoint.GetCategories);

                new FourWeekChallengeManager(repositoryMock.Object, new Mock<IQuestionRandomiser>().Object)
                    .GetHighScores()
                    .Should().BeResultEquivalentTo(expectedValue);
            }

            [Fact]
            public void GivenMemberDoExist_ThenReturnUserScoreInSuccessResult()
            {
                var repositoryMock = new Mock<IGlobalRepository>(MockBehavior.Strict);

                var fakeMember = Fixture.Create<Members>();
                var userScores = Fixture.CreateMany<UserScore>().ToList();

                SetupMockAnyMembersExist(repositoryMock, true);
                SetupMockGetUserScoreInDescendingOrder(repositoryMock, userScores);

                var expectedValue = new Result<List<UserScore>>(userScores)
                    .AddLink("categories", FourWeekChallengeEndpoint.GetCategories);

                new FourWeekChallengeManager(repositoryMock.Object, new Mock<IQuestionRandomiser>().Object)
                    .GetHighScores()
                    .Should().BeResultEquivalentTo(expectedValue);
            }

            private static void SetupMockGetUserScoreInDescendingOrder(Mock<IGlobalRepository> repositoryMock,
                List<UserScore> userScores)
            {
                repositoryMock.Setup(repository => repository.Members.GetUserScoreInDescendingOrder())
                    .Returns(userScores);
            }

            private static void SetupMockAnyMembersExist(Mock<IGlobalRepository> repositoryMock, bool expected)
            {
                repositoryMock.Setup(repository => repository.Members.Any())
                    .Returns(expected);
            }
        }

        public class SubmitPictureAnswerTests : FourWeekChallengeManagerTests
        {
            [Theory]
            [AutoData]
            public void GivenFileTypeIsNotAccepted_ThenReturnFileTypeNotAcceptedErrorResult(string username,
                string fileName, string fileExtension)
            {
                var repositoryMock = new Mock<IGlobalRepository>(MockBehavior.Strict);
                var fileMock = Fixture.Create<Mock<IFormFile>>();

                fileMock.SetupGet(file => file.FileName)
                    .Returns($"{fileName}.{fileExtension}");

                var expectedValue = new Result<bool>(new ErrorMessage(HttpStatusCode.UnprocessableEntity,
                        $"File types are limited to '.jpg','.jpeg' and '.png'. The uploaded file type was .{fileExtension}"))
                    .AddLink(FourWeekChallengeEndpoint.SubmitPictureAnswerRouteWith(username));

                new FourWeekChallengeManager(repositoryMock.Object, new Mock<IQuestionRandomiser>().Object)
                    .SubmitPictureAnswer(username, fileMock.Object)
                    .Should().BeResultEquivalentTo(expectedValue);
            }

            [Theory]
            [InlineAutoData("jpg")]
            [InlineAutoData("jpeg")]
            [InlineAutoData("png")]
            public void GivenFileTypeIsAccepted_WhenSubmitAnswerReturnsSuccessResult_ThenReturnSuccessResult(string fileExtension,
                string username, string fileName)
            {
                var repositoryMock = new Mock<IGlobalRepository>(MockBehavior.Strict);
                var fileMock = Fixture.Create<Mock<IFormFile>>();

                var expectedValue = new Result<bool>(false)
                    .AddLink(FourWeekChallengeEndpoint.GetQuestionRouteWith(username));

                SetupGetMockOnFileName(fileExtension, fileName, fileMock);

                SetupMockForceSubmitAnswerForSuccess(repositoryMock, username);

                new FourWeekChallengeManager(repositoryMock.Object, new Mock<IQuestionRandomiser>().Object)
                    .SubmitPictureAnswer(username, fileMock.Object)
                    .Should().BeResultEquivalentTo(expectedValue);
            }

            //Purely here to result in any form of success in the submit answer method since it is not possible to mock it.
            //Unit tests in this class is for SubmitPictureAnswer method, not SubmitAnswer method.
            private void SetupMockForceSubmitAnswerForSuccess(Mock<IGlobalRepository> repositoryMock, string username)
            {
                var member = Fixture.Build<Members>()
                    .With(members => members.CurrentQuestionNumber, 0)
                    .With(members => members.QuestionIds, BuildQuestionIds(10))
                    .Create();

                var question = Fixture.Build<Questions>()
                    .With(questions => questions.Discriminator, QuestionComplexity.Simple.ToString)
                    .Create();
                var currentQuestionId = int.Parse(member.QuestionIds.Split(",")[member.CurrentQuestionNumber]);

                SetupMockGetMemberOrNull(repositoryMock, username, member);
                SetupMockGetQuestionOrNull(repositoryMock, currentQuestionId, question);
                SetupMockGetComplexQuestion(repositoryMock, currentQuestionId, null);

            }

            private static void SetupGetMockOnFileName(string fileExtension, string fileName, Mock<IFormFile> fileMock)
            {
                fileMock.SetupGet(file => file.FileName)
                    .Returns($"{fileName}.{fileExtension}");
            }
        }

        public class SubmitAnswerTests : FourWeekChallengeManagerTests
        {
            [Theory]
            [AutoData]
            public void GivenUsernameDoesNotExist_ThenReturnMemberDoesNotExistErrorResult(string username,
                string answer)
            {
                var repositoryMock = new Mock<IGlobalRepository>(MockBehavior.Strict);

                SetupMockGetMemberOrNull(repositoryMock, username, null);

                var expectedValue = new Result(new ErrorMessage(HttpStatusCode.NotFound,
                        $"Member with username '{username}' does not exist."))
                    .AddLink(FourWeekChallengeEndpoint.GetCategories);

                new FourWeekChallengeManager(repositoryMock.Object, new Mock<IQuestionRandomiser>().Object)
                    .SubmitAnswer(username, answer)
                    .Should().BeResultEquivalentTo(expectedValue);
            }

            [Theory]
            [AutoData]
            public void GivenCurrentQuestionNumberIsOutsideTheRangeOfIndexesInQuestionIds_ThenReturnIndexOutOfRangeErrorResult(string answer)
            {
                var repositoryMock = new Mock<IGlobalRepository>(MockBehavior.Strict);

                var fakeMember = Fixture.Build<Members>()
                    .With(members => members.QuestionIds, () => BuildQuestionIds(10))
                    .Create();
                ;
                fakeMember.CurrentQuestionNumber = fakeMember.QuestionIds.Split(",").Length + 1;

                SetupMockGetMemberOrNull(repositoryMock, fakeMember.Username, fakeMember);

                var expectedValue = new Result<bool>(new ErrorMessage(HttpStatusCode.InternalServerError,
                    $"Index '{nameof(fakeMember.CurrentQuestionNumber)}' was outside the range for the number of questionIds the member has."));

                new FourWeekChallengeManager(repositoryMock.Object, new Mock<IQuestionRandomiser>().Object)
                    .SubmitAnswer(fakeMember.Username, answer)
                    .Should().BeResultEquivalentTo(expectedValue);
            }

            [Theory]
            [AutoData]
            public void GivenCurrentQuestionNumberIsEqualToQuestionIdsLength_ThenReturnGameHasEndedErrorResult(string answer)
            {
                var repositoryMock = new Mock<IGlobalRepository>(MockBehavior.Strict);

                var fakeMember = Fixture.Build<Members>()
                    .With(members => members.QuestionIds, BuildQuestionIds(10))
                    .Create();

                fakeMember.CurrentQuestionNumber = fakeMember.QuestionIds.Split(",").Length;

                SetupMockGetMemberOrNull(repositoryMock, fakeMember.Username, fakeMember);

                var expectedValue = new Result<bool>(new ErrorMessage(HttpStatusCode.NotFound, $"Game has ended for member with name '{fakeMember.Username}'."))
                    .AddLink(FourWeekChallengeEndpoint.EndGameRouteWith(fakeMember.Username));

                new FourWeekChallengeManager(repositoryMock.Object, new Mock<IQuestionRandomiser>().Object)
                    .SubmitAnswer(fakeMember.Username, answer)
                    .Should().BeResultEquivalentTo(expectedValue);
            }

            [Theory]
            [AutoData]
            public void GivenQuestionIdDoesNotMatchAQuestion_ThenReturnQuestionWithQuestionIdDoesNotExistErrorResult(string answer)
            {
                var repositoryMock = new Mock<IGlobalRepository>(MockBehavior.Strict);

                var questionIds = BuildQuestionIds(10);

                var fakeMember = Fixture.Build<Members>()
                    .With(members => members.QuestionIds, questionIds)
                    .With(members => members.CurrentQuestionNumber, 0)
                    .Create();

                var currentQuestionId = int.Parse(questionIds.Split(",")[fakeMember.CurrentQuestionNumber]);

                var expectedValue = new Result<bool>(
                        new ErrorMessage(
                            HttpStatusCode.NotFound,
                            $"Question with ID '{currentQuestionId}' does not exist in database."))
                    .AddLink(FourWeekChallengeEndpoint.GetQuestionRouteWith(fakeMember.Username));

                SetupMockGetMemberOrNull(repositoryMock, fakeMember.Username, fakeMember);
                SetupMockGetQuestionOrNull(repositoryMock, currentQuestionId, null);

                new FourWeekChallengeManager(repositoryMock.Object, new Mock<IQuestionRandomiser>().Object)
                    .SubmitAnswer(fakeMember.Username, answer)
                    .Should().BeResultEquivalentTo(expectedValue);
            }

            [Theory]
            [AutoData]
            public void GivenAnswerIsIncorrect_ThenReturnSuccessResultWithoutIncrementingQuestionNumberOrUserScore(
                string username, string answer)
            {
                var repositoryMock = new Mock<IGlobalRepository>(MockBehavior.Strict);

                var fakeQuestion = Fixture.Build<Questions>()
                    .With(question => question.Discriminator, QuestionComplexity.Simple.ToString)
                    .Create();

                var questionIds = BuildQuestionIds(10);

                var fakeMemberMock = Fixture.Create<Mock<Members>>();
                var fakeMemberObj = fakeMemberMock.Object;
                fakeMemberObj.Username = username;
                fakeMemberObj.QuestionIds = questionIds;
                fakeMemberObj.CurrentQuestionNumber = 0;

                var oldQuestionNumber = fakeMemberObj.CurrentQuestionNumber;
                var oldUserScore = fakeMemberObj.UserScore;

                var currentQuestionId = int.Parse(questionIds.Split(",")[fakeMemberObj.CurrentQuestionNumber]);

                SetupMockGetMemberOrNull(repositoryMock, fakeMemberObj.Username, fakeMemberObj);
                SetupMockGetQuestionOrNull(repositoryMock, currentQuestionId, fakeQuestion);

                var expectedValue = new Result<bool>(false)
                    .AddLink(FourWeekChallengeEndpoint.GetQuestionRouteWith(fakeMemberObj.Username));

                new FourWeekChallengeManager(repositoryMock.Object, new Mock<IQuestionRandomiser>().Object)
                    .SubmitAnswer(fakeMemberObj.Username, answer)
                    .Should().BeResultEquivalentTo(expectedValue);

                repositoryMock.Verify(repository => repository.SaveChanges(), Times.Never);

                Assert.True(oldQuestionNumber == fakeMemberObj.CurrentQuestionNumber);
                Assert.True(oldUserScore == fakeMemberObj.UserScore);
            }

            [Theory]
            [InlineAutoData("pass", QuestionComplexity.Simple)]
            [InlineAutoData("PaSs", QuestionComplexity.Simple)]
            [InlineAutoData("PASS", QuestionComplexity.Simple)]
            [InlineAutoData("pass", QuestionComplexity.Complex)]
            [InlineAutoData("PaSs", QuestionComplexity.Complex)]
            [InlineAutoData("PASS", QuestionComplexity.Complex)]
            public void GivenAnswerIsPass_ThenReturnSuccessResultAndIncrementCurrentQuestionNumber(
                string answer, QuestionComplexity complexity, string username)
            {
                var repositoryMock = new Mock<IGlobalRepository>(MockBehavior.Strict);

                var fakeQuestion = Fixture.Build<Questions>()
                    .With(question => question.Discriminator, complexity.ToString)
                    .Create();

                var fakeComplexQuestion = Fixture.Build<ComplexQuestions>()
                    .With(complexQuestion => complexQuestion.Discriminator, complexity.ToString)
                    .Create();

                var questionIds = BuildQuestionIds(10);

                var fakeMemberMock = Fixture.Create<Mock<Members>>();
                var fakeMemberObj = fakeMemberMock.Object;
                fakeMemberObj.Username = username;
                fakeMemberObj.QuestionIds = questionIds;
                fakeMemberObj.CurrentQuestionNumber = 0;

                var oldQuestionNumber = fakeMemberObj.CurrentQuestionNumber;
                var oldUserScore = fakeMemberObj.UserScore;

                var currentQuestionId = int.Parse(questionIds.Split(",")[fakeMemberObj.CurrentQuestionNumber]);

                SetupMockGetMemberOrNull(repositoryMock, fakeMemberObj.Username, fakeMemberObj);
                SetupMockGetQuestionOrNull(repositoryMock, currentQuestionId, fakeQuestion);
                if(complexity == QuestionComplexity.Complex) SetupMockGetComplexQuestion(repositoryMock, fakeQuestion.QuestionId, fakeComplexQuestion);
                SetupMockSaveChanges(repositoryMock);

                var expectedValue = new Result<bool>(true)
                    .AddLink(FourWeekChallengeEndpoint.GetQuestionRouteWith(fakeMemberObj.Username));

                new FourWeekChallengeManager(repositoryMock.Object, new Mock<IQuestionRandomiser>().Object)
                    .SubmitAnswer(fakeMemberObj.Username, answer)
                    .Should().BeResultEquivalentTo(expectedValue);

                repositoryMock.Verify(repository => repository.SaveChanges(), Times.Once);

                Assert.True(oldQuestionNumber + 1 == fakeMemberObj.CurrentQuestionNumber);
                Assert.True(oldUserScore == fakeMemberObj.UserScore);
            }

            [Theory]
            [AutoData]
            public void GivenQuestionComplexityIsComplexAndAnswerIsCorrectLocationIsNull_ThenReturnErrorResultLocationNotSet(
              string username, string answer)
            {
                var repositoryMock = new Mock<IGlobalRepository>(MockBehavior.Strict);

                var fakeQuestion = Fixture.Build<Questions>()
                    .With(question => question.Discriminator, QuestionComplexity.Complex.ToString)
                    .With(question => question.CorrectAnswer, answer)
                    .Create();

                var fakeComplexQuestion = Fixture.Build<ComplexQuestions>()
                    .With(complexQuestion => complexQuestion.Discriminator, QuestionComplexity.Complex.ToString)
                    .Create();

                var fakeLocation = (Locations)null;

                var questionIds = BuildQuestionIds(10);

                var fakeMemberMock = Fixture.Create<Mock<Members>>();
                var fakeMemberObj = fakeMemberMock.Object;
                fakeMemberObj.Username = username;
                fakeMemberObj.QuestionIds = questionIds;
                fakeMemberObj.CurrentQuestionNumber = 0;

                var oldQuestionNumber = fakeMemberObj.CurrentQuestionNumber;
                var oldUserScore = fakeMemberObj.UserScore;

                var currentQuestionId = int.Parse(questionIds.Split(",")[fakeMemberObj.CurrentQuestionNumber]);

                SetupMockGetMemberOrNull(repositoryMock, fakeMemberObj.Username, fakeMemberObj);
                SetupMockGetQuestionOrNull(repositoryMock, currentQuestionId, fakeQuestion);
                SetupMockGetComplexQuestion(repositoryMock, fakeQuestion.QuestionId, fakeComplexQuestion);
                SetupMockGetLocationByLocationIdOrNull(repositoryMock, fakeMemberObj, fakeLocation);

                var expectedValue = new Result<bool>(
                        new ErrorMessage(HttpStatusCode.NotFound,
                            $"Member with username '{fakeMemberObj.Username}' does not have a location set."))
                    .AddLink(FourWeekChallengeEndpoint.SetUserLocationRouteWith(fakeMemberObj.Username));

                new FourWeekChallengeManager(repositoryMock.Object, new Mock<IQuestionRandomiser>().Object)
                    .SubmitAnswer(fakeMemberObj.Username, answer)
                    .Should().BeResultEquivalentTo(expectedValue);

                repositoryMock.Verify(repository => repository.SaveChanges(), Times.Never);

                Assert.True(oldQuestionNumber == fakeMemberObj.CurrentQuestionNumber);
                Assert.True(oldUserScore == fakeMemberObj.UserScore);
            }

            [Theory]
            [AutoData]
            public void GivenQuestionComplexityIsComplexAndAnswerIsCorrectLocationIsIncorrect_ThenReturnSuccessResultWithoutIncrementingQuestionNumberOrUserScore(
                string username, string answer)
            {
                var repositoryMock = new Mock<IGlobalRepository>(MockBehavior.Strict);

                var fakeQuestion = Fixture.Build<Questions>()
                    .With(question => question.Discriminator, QuestionComplexity.Complex.ToString)
                    .With(question => question.CorrectAnswer, answer)
                    .Create();

                var fakeComplexQuestion = Fixture.Build<ComplexQuestions>()
                    .With(complexQuestion => complexQuestion.Discriminator, QuestionComplexity.Complex.ToString)
                    .Create();

                var fakeLocation = Fixture.Create<Locations>();

                var questionIds = BuildQuestionIds(10);

                var fakeMemberMock = Fixture.Create<Mock<Members>>();
                var fakeMemberObj = fakeMemberMock.Object;
                fakeMemberObj.Username = username;
                fakeMemberObj.QuestionIds = questionIds;
                fakeMemberObj.CurrentQuestionNumber = 0;

                var oldQuestionNumber = fakeMemberObj.CurrentQuestionNumber;
                var oldUserScore = fakeMemberObj.UserScore;

                var currentQuestionId = int.Parse(questionIds.Split(",")[fakeMemberObj.CurrentQuestionNumber]);

                SetupMockGetMemberOrNull(repositoryMock, fakeMemberObj.Username, fakeMemberObj);
                SetupMockGetQuestionOrNull(repositoryMock, currentQuestionId, fakeQuestion);
                SetupMockGetComplexQuestion(repositoryMock, fakeQuestion.QuestionId, fakeComplexQuestion);
                SetupMockGetLocationByLocationIdOrNull(repositoryMock, fakeMemberObj, fakeLocation);
                SetupMockSaveChanges(repositoryMock);

                var expectedValue = new Result<bool>(false)
                    .AddLink(FourWeekChallengeEndpoint.GetQuestionRouteWith(fakeMemberObj.Username));

                new FourWeekChallengeManager(repositoryMock.Object, new Mock<IQuestionRandomiser>().Object)
                    .SubmitAnswer(fakeMemberObj.Username, answer)
                    .Should().BeResultEquivalentTo(expectedValue);

                repositoryMock.Verify(repository => repository.SaveChanges(), Times.Once);

                Assert.True(oldQuestionNumber == fakeMemberObj.CurrentQuestionNumber);
                Assert.True(oldUserScore == fakeMemberObj.UserScore);
            }

            [Theory]
            [AutoData]
            public void GivenQuestionComplexityIsComplexAndAnswerIsCorrectAndHintAsked_ThenReturnSuccessResultWithIncrementingQuestionNumberOrUserScoreByOne(
             string username, string answer)
            {
                var repositoryMock = new Mock<IGlobalRepository>(MockBehavior.Strict);

                var fakeLocation = Fixture.Create<Locations>();

                var fakeQuestion = Fixture.Build<Questions>()
                    .With(question => question.Discriminator, QuestionComplexity.Complex.ToString)
                    .With(question => question.CorrectAnswer, answer)
                    .Create();

                var fakeComplexQuestion = Fixture.Build<ComplexQuestions>()
                    .With(complexQuestion => complexQuestion.Discriminator, QuestionComplexity.Complex.ToString)
                    .With(complexQuestion => complexQuestion.Location, fakeLocation)
                    .Create();

                var questionIds = BuildQuestionIds(10);

                var fakeMemberMock = Fixture.Create<Mock<Members>>();
                var fakeMemberObj = fakeMemberMock.Object;
                fakeMemberObj.Username = username;
                fakeMemberObj.QuestionIds = questionIds;
                fakeMemberObj.CurrentQuestionNumber = 0;
                fakeMemberObj.HintAsked = true;


                var oldQuestionNumber = fakeMemberObj.CurrentQuestionNumber;
                var oldUserScore = fakeMemberObj.UserScore;

                var currentQuestionId = int.Parse(questionIds.Split(",")[fakeMemberObj.CurrentQuestionNumber]);

                SetupMockGetMemberOrNull(repositoryMock, fakeMemberObj.Username, fakeMemberObj);
                SetupMockGetQuestionOrNull(repositoryMock, currentQuestionId, fakeQuestion);
                SetupMockGetComplexQuestion(repositoryMock, fakeQuestion.QuestionId, fakeComplexQuestion);
                SetupMockGetLocationByLocationIdOrNull(repositoryMock, fakeMemberObj, fakeLocation);
                SetupMockSaveChanges(repositoryMock);

                var expectedValue = new Result<bool>(true)
                    .AddLink(FourWeekChallengeEndpoint.GetQuestionRouteWith(fakeMemberObj.Username));

                new FourWeekChallengeManager(repositoryMock.Object, new Mock<IQuestionRandomiser>().Object)
                    .SubmitAnswer(fakeMemberObj.Username, answer)
                    .Should().BeResultEquivalentTo(expectedValue);

                repositoryMock.Verify(repository => repository.SaveChanges(), Times.Once);

                Assert.True(oldQuestionNumber + 1 == fakeMemberObj.CurrentQuestionNumber);
                Assert.True(oldUserScore + 1 == fakeMemberObj.UserScore);
            }

            [Theory]
            [AutoData]
            public void GivenQuestionComplexityIsComplexAndAnswerIsCorrectAndLocationHintAsked_ThenReturnSuccessResultWithIncrementingQuestionNumberOrUserScoreByOne(
            string username, string answer)
            {
                var repositoryMock = new Mock<IGlobalRepository>(MockBehavior.Strict);

                var fakeLocation = Fixture.Create<Locations>();

                var fakeQuestion = Fixture.Build<Questions>()
                    .With(question => question.Discriminator, QuestionComplexity.Complex.ToString)
                    .With(question => question.CorrectAnswer, answer)
                    .Create();

                var fakeComplexQuestion = Fixture.Build<ComplexQuestions>()
                    .With(complexQuestion => complexQuestion.Discriminator, QuestionComplexity.Complex.ToString)
                    .With(complexQuestion => complexQuestion.Location, fakeLocation)
                    .Create();

                var questionIds = BuildQuestionIds(10);

                var fakeMemberMock = Fixture.Create<Mock<Members>>();
                var fakeMemberObj = fakeMemberMock.Object;
                fakeMemberObj.Username = username;
                fakeMemberObj.QuestionIds = questionIds;
                fakeMemberObj.CurrentQuestionNumber = 0;
                fakeMemberObj.LocationHintAsked = true;


                var oldQuestionNumber = fakeMemberObj.CurrentQuestionNumber;
                var oldUserScore = fakeMemberObj.UserScore;

                var currentQuestionId = int.Parse(questionIds.Split(",")[fakeMemberObj.CurrentQuestionNumber]);

                SetupMockGetMemberOrNull(repositoryMock, fakeMemberObj.Username, fakeMemberObj);
                SetupMockGetQuestionOrNull(repositoryMock, currentQuestionId, fakeQuestion);
                SetupMockGetComplexQuestion(repositoryMock, fakeQuestion.QuestionId, fakeComplexQuestion);
                SetupMockGetLocationByLocationIdOrNull(repositoryMock, fakeMemberObj, fakeLocation);
                SetupMockSaveChanges(repositoryMock);

                var expectedValue = new Result<bool>(true)
                    .AddLink(FourWeekChallengeEndpoint.GetQuestionRouteWith(fakeMemberObj.Username));

                new FourWeekChallengeManager(repositoryMock.Object, new Mock<IQuestionRandomiser>().Object)
                    .SubmitAnswer(fakeMemberObj.Username, answer)
                    .Should().BeResultEquivalentTo(expectedValue);

                repositoryMock.Verify(repository => repository.SaveChanges(), Times.Once);

                Assert.True(oldQuestionNumber + 1 == fakeMemberObj.CurrentQuestionNumber);
                Assert.True(oldUserScore + 1 == fakeMemberObj.UserScore);
            }

            [Theory]
            [AutoData]
            public void GivenQuestionComplexityIsComplexAndAnswerIsCorrectAndNoHintAsked_ThenReturnSuccessResultWithIncrementingQuestionNumberOrUserScoreByTwo(
                string username, string answer)
            {
                var repositoryMock = new Mock<IGlobalRepository>(MockBehavior.Strict);

                var fakeLocation = Fixture.Create<Locations>();

                var fakeQuestion = Fixture.Build<Questions>()
                    .With(question => question.Discriminator, QuestionComplexity.Complex.ToString)
                    .With(question => question.CorrectAnswer, answer)
                    .Create();

                var fakeComplexQuestion = Fixture.Build<ComplexQuestions>()
                    .With(complexQuestion => complexQuestion.Discriminator, QuestionComplexity.Complex.ToString)
                    .With(complexQuestion => complexQuestion.Location, fakeLocation)
                    .Create();

                var questionIds = BuildQuestionIds(10);

                var fakeMemberMock = Fixture.Create<Mock<Members>>();
                var fakeMemberObj = fakeMemberMock.Object;
                fakeMemberObj.Username = username;
                fakeMemberObj.QuestionIds = questionIds;
                fakeMemberObj.CurrentQuestionNumber = 0;

                var oldQuestionNumber = fakeMemberObj.CurrentQuestionNumber;
                var oldUserScore = fakeMemberObj.UserScore;

                var currentQuestionId = int.Parse(questionIds.Split(",")[fakeMemberObj.CurrentQuestionNumber]);

                SetupMockGetMemberOrNull(repositoryMock, fakeMemberObj.Username, fakeMemberObj);
                SetupMockGetQuestionOrNull(repositoryMock, currentQuestionId, fakeQuestion);
                SetupMockGetComplexQuestion(repositoryMock, fakeQuestion.QuestionId, fakeComplexQuestion);
                SetupMockGetLocationByLocationIdOrNull(repositoryMock, fakeMemberObj, fakeLocation);
                SetupMockSaveChanges(repositoryMock);

                var expectedValue = new Result<bool>(true)
                    .AddLink(FourWeekChallengeEndpoint.GetQuestionRouteWith(fakeMemberObj.Username));

                new FourWeekChallengeManager(repositoryMock.Object, new Mock<IQuestionRandomiser>().Object)
                    .SubmitAnswer(fakeMemberObj.Username, answer)
                    .Should().BeResultEquivalentTo(expectedValue);

                repositoryMock.Verify(repository => repository.SaveChanges(), Times.Once);

                Assert.True(oldQuestionNumber + 1 == fakeMemberObj.CurrentQuestionNumber);
                Assert.True(oldUserScore + 2 == fakeMemberObj.UserScore);
            }

            [Theory]
            [AutoData]
            public void GivenGameHasEnded_ThenReturnSuccessResultWithEndGameRouteLink(
              string username, string answer)
            {
                var repositoryMock = new Mock<IGlobalRepository>(MockBehavior.Strict);

                var fakeLocation = Fixture.Create<Locations>();

                var fakeQuestion = Fixture.Build<Questions>()
                    .With(question => question.Discriminator, QuestionComplexity.Complex.ToString)
                    .With(question => question.CorrectAnswer, answer)
                    .Create();

                var fakeComplexQuestion = Fixture.Build<ComplexQuestions>()
                    .With(complexQuestion => complexQuestion.Discriminator, QuestionComplexity.Complex.ToString)
                    .With(complexQuestion => complexQuestion.Location, fakeLocation)
                    .Create();

                var questionIds = BuildQuestionIds(10);

                var fakeMemberMock = Fixture.Create<Mock<Members>>();
                var fakeMemberObj = fakeMemberMock.Object;
                fakeMemberObj.Username = username;
                fakeMemberObj.QuestionIds = questionIds;
                fakeMemberObj.CurrentQuestionNumber = questionIds.Split(",").Length - 1;

                var oldQuestionNumber = fakeMemberObj.CurrentQuestionNumber;
                var oldUserScore = fakeMemberObj.UserScore;

                var currentQuestionId = int.Parse(questionIds.Split(",")[fakeMemberObj.CurrentQuestionNumber]);

                SetupMockGetMemberOrNull(repositoryMock, fakeMemberObj.Username, fakeMemberObj);
                SetupMockGetQuestionOrNull(repositoryMock, currentQuestionId, fakeQuestion);
                SetupMockGetComplexQuestion(repositoryMock, fakeQuestion.QuestionId, fakeComplexQuestion);
                SetupMockGetLocationByLocationIdOrNull(repositoryMock, fakeMemberObj, fakeLocation);
                SetupMockSaveChanges(repositoryMock);

                var expectedValue = new Result<bool>(true)
                    .AddLink(FourWeekChallengeEndpoint.EndGameRouteWith(fakeMemberObj.Username));

                new FourWeekChallengeManager(repositoryMock.Object, new Mock<IQuestionRandomiser>().Object)
                    .SubmitAnswer(fakeMemberObj.Username, answer)
                    .Should().BeResultEquivalentTo(expectedValue);

                repositoryMock.Verify(repository => repository.SaveChanges(), Times.Once);

                Assert.True(oldQuestionNumber + 1 == fakeMemberObj.CurrentQuestionNumber);
                Assert.True(oldUserScore + 2 == fakeMemberObj.UserScore);
            }

            private static void SetupMockGetLocationByLocationIdOrNull(Mock<IGlobalRepository> repositoryMock, Members fakeMemberObj,
                Locations location)
            {
                repositoryMock.Setup(repository =>
                        repository.Locations.GetLocationByLocationIdOrNull(fakeMemberObj.LocationId))
                    .Returns(location);
            }
        }

        protected static void SetupMockGetMemberOrNull(Mock<IGlobalRepository> repositoryMock,
            string username,
            Members expected)
        {
            repositoryMock.Setup(repository => repository.Members.GetMemberOrNull(username))
                .Returns(() => expected);
        }

        protected static void SetupMockDoesQuestionExist(Mock<IGlobalRepository> repositoryMock, int currentQuestionId,
            bool expected)
        {
            repositoryMock.Setup(repository => repository.Questions.DoesQuestionExist(currentQuestionId))
                .Returns(expected);
        }

        protected static void SetupMockGetComplexQuestion(Mock<IGlobalRepository> repositoryMock, int currentQuestionId,
            ComplexQuestions expected)
        {
            repositoryMock.Setup(repository => repository.ComplexQuestions.GetComplexQuestion(currentQuestionId))
                .Returns(() => expected);
        }
        protected static void SetupMockGetQuestionOrNull(Mock<IGlobalRepository> repositoryMock, int currentQuestionId, Questions? expected)
        {
            repositoryMock.Setup(repository => repository.Questions.GetQuestionOrNull(currentQuestionId))
                .Returns(() => expected);
        }

        protected static void SetupMockSaveChanges(Mock<IGlobalRepository> repositoryMock)
        {
            repositoryMock.Setup(repository => repository.SaveChanges());
        }

        protected string BuildQuestionIds(int count)
        {
            return string.Join(",", Fixture.CreateMany<int>(count));

        }
    }
}