using Assignment4WC.API.Controllers.Models;
using FluentValidation;

namespace Assignment4WC.API.Controllers.Validators
{
    public class LocationValidator : AbstractValidator<LocationDto>
    {
        public LocationValidator()
        {
            RuleFor(dto => dto.Latitude)
                .GreaterThanOrEqualTo(-90)
                .LessThanOrEqualTo(90)
                .NotEmpty();

            RuleFor(dto => dto.Longitude)
                .GreaterThanOrEqualTo(-180)
                .LessThanOrEqualTo(180)
                .NotEmpty();
        }
    }
}
