using System;
using System.ComponentModel.DataAnnotations;
using Assignment4WC.API.Controllers.Models;
using Assignment4WC.API.Controllers.Validators;
using Assignment4WC.Context;
using Assignment4WC.Models;
using AutoFixture;
using AutoFixture.Xunit2;
using FluentValidation.TestHelper;
using Xunit;

namespace Assignment4WC.Tests.API.Validators
{
    public class LocationValidatorTests : IDisposable
    {
        protected readonly AssignmentContext Context;
        protected readonly IFixture Fixture;
        public LocationValidatorTests()
        {
            Context = new AssignmentContext(AssignmentContextOptions.GetDbOptions());
            Fixture = new Fixture();
        }

        [Fact]
        public void GivenLatitudeIsNullOrEmpty_ReturnValidationErrorWithLatitudeIsEmptyError()
        {
            var dtoModel = Fixture.Build<LocationDto>()
                .With(dto => dto.Latitude, () => default)
                .Create();

            new LocationValidator().TestValidate(dtoModel)
                .ShouldHaveValidationErrorFor(dto => dto.Latitude)
                .WithErrorMessage("'Latitude' must not be empty.");
        }

        [Theory]
        [InlineData(-91)]
        [InlineData(-178)]
        [InlineData(-1000)]
        [InlineData(-43000)]
        [InlineData(int.MinValue)]
        public void GivenLatitudeIsBelowBounds_ReturnValidationErrorWithLatitudeMustBeGreaterThanOrEqualToBoundary(decimal latitude)
        {
            var dtoModel = Fixture.Build<LocationDto>()
                .With(dto => dto.Latitude, () => latitude)
                .Create();

            new LocationValidator().TestValidate(dtoModel)
                .ShouldHaveValidationErrorFor(dto => dto.Latitude)
                .WithErrorMessage("'Latitude' must be greater than or equal to '-90'.");
        }

        [Theory]
        [InlineData(91)]
        [InlineData(178)]
        [InlineData(1000)]
        [InlineData(43000)]
        [InlineData(int.MaxValue)]
        public void GivenLatitudeIsAboveBounds_ReturnValidationErrorWithLatitudeMustBeLessThanOrEqualToBoundary(decimal latitude)
        {
            var dtoModel = Fixture.Build<LocationDto>()
                .With(dto => dto.Latitude, () => latitude)
                .Create();

            new LocationValidator().TestValidate(dtoModel)
                .ShouldHaveValidationErrorFor(dto => dto.Latitude)
                .WithErrorMessage("'Latitude' must be less than or equal to '90'.");
        }


        [Theory]
        [AutoData]
        public void GivenLatitudeIsValid_ThenThereShouldBeNoValidationErrorForLatitude([Range(-90, 90)] decimal latitude)
        {
            var dtoModel = Fixture.Build<LocationDto>()
                .With(dto => dto.Latitude, () => latitude)
                .Create();

            new LocationValidator().TestValidate(dtoModel)
                .ShouldNotHaveValidationErrorFor(dto => dto.Latitude);
        }

        [Theory]
        [InlineData(-181)]
        [InlineData(-200)]
        [InlineData(-1000)]
        [InlineData(-43000)]
        [InlineData(int.MinValue)]
        public void GivenLongitudeIsBelowBounds_ReturnValidationErrorWithLongitudeMustBeGreaterThanOrEqualToBoundary(decimal longitude)
        {
            var dtoModel = Fixture.Build<LocationDto>()
                .With(dto => dto.Longitude, () => longitude)
                .Create();

            new LocationValidator().TestValidate(dtoModel)
                .ShouldHaveValidationErrorFor(dto => dto.Longitude)
                .WithErrorMessage("'Longitude' must be greater than or equal to '-180'.");
        }

        [Theory]
        [InlineData(181)]
        [InlineData(200)]
        [InlineData(1000)]
        [InlineData(43000)]
        [InlineData(int.MaxValue)]
        public void GivenLongitudeIsAboveBounds_ReturnValidationErrorWithLongitudeMustBeLessThanOrEqualToBoundary(decimal longitude)
        {
            var dtoModel = Fixture.Build<LocationDto>()
                .With(dto => dto.Longitude, () => longitude)
                .Create();

            new LocationValidator().TestValidate(dtoModel)
                .ShouldHaveValidationErrorFor(dto => dto.Longitude)
                .WithErrorMessage("'Longitude' must be less than or equal to '180'.");
        }


        [Theory]
        [AutoData]
        public void GivenLongitudeIsValid_ThenThereShouldBeNoValidationErrorForLongitude([Range(-90, 90)] decimal longitude)
        {
            var dtoModel = Fixture.Build<LocationDto>()
                .With(dto => dto.Longitude, () => longitude)
                .Create();

            new LocationValidator().TestValidate(dtoModel)
                .ShouldNotHaveValidationErrorFor(dto => dto.Longitude);
        }

        public void Dispose()
        {
            Context.Dispose();
        }
    }
}