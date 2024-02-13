using FluentValidation;
using Integration.Core.Features.Commands;

namespace Integration.Core.Features.Validators;

public class InclusaoContaCommandValidator : AbstractValidator<InclusaoContaCommand>
{
    public InclusaoContaCommandValidator()
    {
        RuleFor(x => x.NomeTitular).NotEmpty();
    }
}
