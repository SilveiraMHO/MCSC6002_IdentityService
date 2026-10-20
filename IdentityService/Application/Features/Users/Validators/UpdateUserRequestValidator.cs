using Application.Features.Users.Commands;
using FluentValidation;
using ResponseLibrary.Models.Requests.Identity;

namespace Application.Features.Users.Validators
{
    public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
    {
        public UpdateUserCommandValidator()
        {
            RuleFor(command => command.UpdateUser)
                .SetValidator(new UpdateUserRequestValidator());
        }
    }

    public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
    {
        public UpdateUserRequestValidator()
        {
            RuleFor(x => x.FirstName)
                .MinimumLength(4)
                .MaximumLength(20)
                .NotEmpty();
            RuleFor(x => x.LastName)
                .MinimumLength(4)
                .MaximumLength(20)
                .NotEmpty();
            RuleFor(x => x.PhoneNumber)
                .MinimumLength(10)
                .MaximumLength(15)
                .NotEmpty();
        }
    }
}
