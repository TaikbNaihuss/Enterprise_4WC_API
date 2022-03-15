using Assignment4WC.API.Controllers.Models;
using FluentValidation;

namespace Assignment4WC.API.Controllers.Validators
{
    public class AnswerValidator : AbstractValidator<AnswerDto>
    {
        public AnswerValidator()
        {
            RuleFor(dto => dto.Answer)
                .NotEmpty()
                .NotNull();
        }
    }
}
