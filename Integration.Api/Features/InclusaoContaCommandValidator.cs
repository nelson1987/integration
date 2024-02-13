using FluentValidation;

namespace Integration.Api.Features;

public class InclusaoContaCommandValidator : AbstractValidator<InclusaoContaCommand>
{
    public InclusaoContaCommandValidator()
    {
        RuleFor(x => x.NomeTitular).NotEmpty();
    }
}
