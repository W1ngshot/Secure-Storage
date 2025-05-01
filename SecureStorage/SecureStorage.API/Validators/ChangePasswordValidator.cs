using FastEndpoints;
using FluentValidation;
using SecureStorage.API.Models;

namespace SecureStorage.API.Validators;

public class ChangePasswordValidator : Validator<ChangePasswordRequest>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.OldPassword)
            .NotEmpty()
            .WithMessage("required");
        
        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithMessage("required");
    }
}