using System;
using Assignment4WC.API.Controllers.Models;
using Assignment4WC.API.Controllers.Validators;
using Assignment4WC.Context;
using Assignment4WC.Models;
using AutoFixture;
using FluentValidation.TestHelper;
using Xunit;

namespace Assignment4WC.Tests.API.Validators
{
    public class InitialDetailsValidatorTests : IDisposable
    {
        protected readonly AssignmentContext Context;
        protected readonly IFixture Fixture;
        public InitialDetailsValidatorTests()
        {
            Context = new AssignmentContext(AssignmentContextOptions.GetDbOptions());
            Fixture = new Fixture();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("      ")]
        public void GivenUsernameIsNullOrEmptyOrWhitespace_ReturnValidationErrorWithUsernameIsEmptyError(string username)
        {
            var dtoModel = Fixture.Build<InitialDetailsDto>()
                .With(dto => dto.Username, () => username)
                .Create();

            new InitialDetailsValidator().TestValidate(dtoModel)
                .ShouldHaveValidationErrorFor(dto => dto.Username)
                .WithErrorMessage("'Username' must not be empty.");
        }

        [Fact]
        public void GivenUsernameIsValid_ThenThereShouldBeNoValidationErrorForUsername()
        {
            var dtoModel = Fixture.Build<InitialDetailsDto>()
                .With(dto => dto.Username, () => Fixture.Create<string>())
                .Create();

            new InitialDetailsValidator().TestValidate(dtoModel)
                .ShouldNotHaveValidationErrorFor(dto => dto.Username);
        }


        [Fact]
        public void GivenCategoryNameIsNotAValidCategory_ReturnValidationErrorWithCategoryIsNotValidError()
        {
            var dtoModel = Fixture.Build<InitialDetailsDto>()
                .With(dto => dto.Category, () => Fixture.Create<string>())
                .Create();

            new InitialDetailsValidator().TestValidate(dtoModel)
                .ShouldHaveValidationErrorFor(dto => dto.Category)
                .WithErrorMessage($"'{dtoModel.Category}' is not a valid Category");
        }

        [Fact]
        public void GivenCategoryNameIsAValidCategory_ThenThereShouldBeNoValidationErrorForCategory()
        {
            var dtoModel = Fixture.Build<InitialDetailsDto>()
                .With(dto => dto.Category, () => Fixture.Create<CategoryType>().ToString())
                .Create();

            new InitialDetailsValidator().TestValidate(dtoModel)
                .ShouldNotHaveValidationErrorFor(dto => dto.Category);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-24)]
        [InlineData(-42465556)]
        [InlineData(int.MinValue)]
        public void GivenNumOfQuestionsIsLessThanOrEqualToZero_ReturnValidationErrorWithNumOfQuestionsMustBeGreaterThanZero(int numOfQuestions)
        {
            var dtoModel = Fixture.Build<InitialDetailsDto>()
                .With(dto => dto.NumOfQuestions, () => numOfQuestions)
                .Create();

            new InitialDetailsValidator().TestValidate(dtoModel)
                .ShouldHaveValidationErrorFor(dto => dto.NumOfQuestions)
                .WithErrorMessage($"'Num Of Questions' must be greater than '0'.");
        }

        [Fact]
        public void GivenNumOfQuestionsIsGreaterThanZero_ThenThereShouldBeNoValidationErrorForNumOfQuestions()
        {
            var dtoModel = Fixture.Build<InitialDetailsDto>()
                .With(dto => dto.NumOfQuestions, () => Fixture.Create<int>())
                .Create();

            new InitialDetailsValidator().TestValidate(dtoModel)
                .ShouldNotHaveValidationErrorFor(dto => dto.NumOfQuestions);
        }

        public void Dispose()
        {
            Context.Dispose();
        }
    }
}