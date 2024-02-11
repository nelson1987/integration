using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CourseTDD.Api;

public static class PagamentoController
{
    public enum FormaPagamento { CartaoCredito = 1 }

    public static IServiceCollection AddDependencies(this IServiceCollection services)
    {
        services.AddScoped<IContaService, ContaService>();
        services.AddScoped<IValidator<InclusaoContaCommand>, InclusaoContaCommandValidator>();
        return services;
    }

    public static void MetodoPost(this WebApplication app)
    {
        app.MapPost("/", async (
            [FromServices] IContaService service,
            CancellationToken cancellationToken) =>
        {
            return await service.CriarConta(new InclusaoContaCommand("Teste"));
        })
        .WithName("PostTdd")
        .WithOpenApi();
    }
}
public class Conta
{
    public Guid Id { get; set; }
}
public record InclusaoContaCommand(string Nome);
public class InclusaoContaCommandValidator : AbstractValidator<InclusaoContaCommand>
{
    public InclusaoContaCommandValidator()
    {
        RuleFor(x => x.Nome).NotEmpty();
    }
}
public static class FluentValidationExtensions
{
    public static bool IsInvalid(this ValidationResult result) => !result.IsValid;

    public static ModelStateDictionary ToModelState(this ValidationResult result)
    {
        var modelState = new ModelStateDictionary();

        foreach (var error in result.Errors)
            modelState.AddModelError(error.PropertyName, error.ErrorMessage);
        return modelState;
    }

    public static ModelStateDictionary ToModelState(this ValidationFailure result)
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError(result.PropertyName, result.ErrorMessage);

        return modelState;
    }
}
public static class FluentResultsExtensions
{
    public record Error(string Message, IDictionary<string, object>? Metadata);
    public record ErrorResponse(Error[] Errors);

    public static Result ToFailResult(this ValidationResult validationResult)
    {
        var errors = validationResult.Errors.Select(x => new FluentResults.Error(x.ErrorMessage)
            .WithMetadata(nameof(x.PropertyName), x.PropertyName)
            .WithMetadata(nameof(x.AttemptedValue), x.AttemptedValue));

        return Result.Fail(errors);
    }


    public static string[] GetErrorsMessage(this ResultBase result) => result.Errors.Select(x => x.Message).ToArray();
    public static ErrorResponse ToErrorResponse(this ResultBase result, bool skipMetadata = false) =>
        new(result.Errors.Select(x => new Error(x.Message, skipMetadata ? null : x.Metadata)).ToArray());
}
public interface IContaService
{
    Task<Result> CriarConta(InclusaoContaCommand command);
}
public class ContaService : IContaService
{
    private readonly IValidator<InclusaoContaCommand> _validator;

    public ContaService(IValidator<InclusaoContaCommand> validator)
    {
        _validator = validator;
    }

    //private readonly IValidator<InclusaoContaCommand> _validator;
    public Task<Result> CriarConta(InclusaoContaCommand command)
    {
        var paginationHeaderValidation = _validator.Validate(command);
        if (!paginationHeaderValidation.IsValid)
            return Task.FromResult(paginationHeaderValidation.ToFailResult());

        //Conta entity = new Conta();
        //return Task.FromResult(Results.Created($"/{entity.Id}", entity));
        return Task.FromResult(Result.Ok());
    }
}
