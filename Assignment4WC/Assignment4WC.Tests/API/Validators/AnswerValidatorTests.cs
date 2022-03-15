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
    public class AnswerValidatorTests : IDisposable
    {
        protected readonly AssignmentContext Context;
        protected readonly IFixture Fixture;
        public AnswerValidatorTests()
        {
            Context = new AssignmentContext(AssignmentContextOptions.GetDbOptions());
            Fixture = new Fixture();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("      ")]
        public void GivenAnswerIsNullOrEmptyOrWhitespace_ReturnValidationErrorWithAnswerIsEmptyError(string answer)
        {
            var dtoModel = Fixture.Build<AnswerDto>()
                .With(dto => dto.Answer, () => answer)
                .Create();

            new AnswerValidator().TestValidate(dtoModel)
                .ShouldHaveValidationErrorFor(dto => dto.Answer)
                .WithErrorMessage("'Answer' must not be empty.");
        }

        [Fact]
        public void GivenAnswerIsValid_ThenThereShouldBeNoValidationErrorFoAnswer()
        {
            var dtoModel = Fixture.Build<AnswerDto>()
                .With(dto => dto.Answer, () => Fixture.Create<string>())
                .Create();

            new AnswerValidator().TestValidate(dtoModel)
                .ShouldNotHaveValidationErrorFor(dto => dto.Answer);
        }

        public void Dispose()
        {
            Context.Dispose();
        }
    }
}