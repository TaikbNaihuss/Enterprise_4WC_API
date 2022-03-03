using Assignment4WC.API.Controllers.Models;
using Assignment4WC.Models;
using FluentValidation;

namespace Assignment4WC.API.Controllers.Validators
{
    public class InitialDetailsValidator : AbstractValidator<InitialDetailsDto>
    {
        public InitialDetailsValidator()
        {
            RuleFor(details => details.Username)
                .NotNull()
                .NotEmpty();

            RuleFor(details => details.Category)
                .IsEnumName(typeof(CategoryType), false)
                .WithMessage(details => $"'{details.Category}' is not a valid {nameof(details.Category)}");

            RuleFor(details => details.NumOfQuestions)
                .GreaterThan(0);
        }
    }
}
