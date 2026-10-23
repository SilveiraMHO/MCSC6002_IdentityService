using Application.Features.Users.Commands;
using FluentValidation;
using ResponseLibrary.Models.Requests.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Users.Validators
{
    public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {
        public CreateUserCommandValidator()
        {
            RuleFor(command => command.CreateUser)
                .SetValidator(new CreateUserRequestValidator());
        }
    }

    public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
    {
        public CreateUserRequestValidator()
        {
            RuleFor(x => x.FirstName)
                .MinimumLength(4)
                .MaximumLength(20)
                .NotEmpty();
            RuleFor(x => x.LastName)
                .MinimumLength(4)
                .MaximumLength(20)
                .NotEmpty();
            RuleFor(x => x.UserName)
                .MinimumLength(4)
                .MaximumLength(20)
                .NotEmpty();
            RuleFor(x => x.Password)
                .MinimumLength(12) // Matches Identity password policy.
                .MaximumLength(128) // Passphrases OK; caps oversized input.
                .NotEmpty();
            RuleFor(x => x.PhoneNumber)
                .MinimumLength(10)
                .MaximumLength(15)
                .NotEmpty();
            RuleFor(x => x.Email)
                .EmailAddress()
                .NotEmpty();
        }
    }
}
