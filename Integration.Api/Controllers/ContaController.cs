using AutoMapper;
using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MongoDB.Driver;
using MassTransit;

namespace Integration.Api.Controllers;
#region Dependencies
public static class Dependencies
{
    public static IServiceCollection AddCore(this IServiceCollection services)
    {
        //services.AddScoped<IMongoContext, ConsultaFinanceiraContext>();
        //services.AddMongoDb();
        services.AddRabbitMq();
        services.AddScoped<IValidator<InclusaoContaCommand>, InclusaoContaCommandValidator>();

        return services;
    }

    private static IServiceCollection AddRabbitMq(this IServiceCollection services)
    {
        services.AddMassTransit(x =>
        {
        //    x.AddConsumeObserver<ConsumeObserver>();
        //    x.AddSendObserver<SendObserver>();
            x.SetKebabCaseEndpointNameFormatter();
        //    x.AddConsumer<ContaCriadaConsumer>();
            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host("amqp://guest:guest@localhost:5672");
                cfg.ConfigureEndpoints(ctx);
                //cfg.UseRawJsonSerializer();
            });
        });
        return services;
    }
    private static IServiceCollection AddMongoDb(this IServiceCollection services)
    {
        var settings = new
        {
            MongoClient = "mongodb://root:example@localhost:27017/",
            Database = "sales"
        };
        var mongoClient = new MongoClient(settings.MongoClient);
        services.AddSingleton(mongoClient.GetDatabase(settings.Database));
        return services;
    }
}
#endregion

#region Core
public record InclusaoContaCommand
{
    public required string NumeroContaDebitada { get; set; }

}
public static class ObjectMapper
{
    private static readonly Lazy<IMapper> Lazy = new(() =>
    {
        var config = new MapperConfiguration(cfg => cfg.AddMaps(typeof(ObjectMapper).Assembly));
        return config.CreateMapper();
    });

    public static IMapper Mapper => Lazy.Value;

    public static T MapTo<T>(this object source) => Mapper.Map<T>(source);
}
public class InclusaoContaCommandValidator : AbstractValidator<InclusaoContaCommand>
{
    public InclusaoContaCommandValidator()
    {
        RuleFor(x => x.NumeroContaDebitada).NotEmpty();
    }
}
public interface IProducer
{
    Task Send(ContaIncluidaEvent @event, CancellationToken cancellationToken);
}
public class ContaProducer : IProducer
{
    private readonly IBus _bus;

    public ContaProducer(IBus bus)
    {
        _bus = bus;
    }
    public async Task Send(ContaIncluidaEvent @event, CancellationToken cancellationToken)
    {
        await _bus.Send(@event);
    }
}
public record ContaIncluidaEvent
{
    public Guid Id { get; set; }
    public required string Numero { get; set; }
    public required string Documento { get; set; }
    public decimal Saldo { get; set; }
    public required string Titular { get; set; }
    public required List<Transacao> Transacoes { get; set; }
}
public interface IDataReader
{
    Task Insert(Conta conta, CancellationToken cancellationToken);
}
public interface IMongoContext
{
    public IMongoCollection<Conta> Contas { get; }
    public IMongoCollection<Transacao> Transacoes { get; }
}

public class ConsultaFinanceiraContext : IMongoContext
{
    private readonly IMongoClient _client;
    private readonly IMongoDatabase _database;

    public ConsultaFinanceiraContext(string connectionString, string databaseName)
    {
        _client = new MongoClient(connectionString);
        _database = _client.GetDatabase(databaseName);
    }

    public IMongoCollection<Conta> Contas => _database.GetCollection<Conta>("contas");
    public IMongoCollection<Transacao> Transacoes => _database.GetCollection<Transacao>("transacoes");
}
public class Conta
{
    public Guid Id { get; set; }
    public required string Numero { get; set; }
    public required string Documento { get; set; }
    public decimal Saldo { get; set; }
    public required string Titular { get; set; }
    public required List<Transacao> Transacoes { get; set; }
}

public class Transacao
{
    public int Id { get; set; }
    public DateTime Data { get; set; }
    public decimal Valor { get; set; }
    public required string Tipo { get; set; }
    public required string Descricao { get; set; }
}
#endregion

public class AutomapperProfile : Profile
{
    public AutomapperProfile()
    {
        CreateMap<InclusaoContaCommand, Conta>();
        CreateMap<Conta, ContaIncluidaEvent>();
        //.ForMember(x => x.Id, y => y.MapFrom());
    }
}
public static class FluentValidationExtensions
{
    public static bool IsInvalid(this FluentValidation.Results.ValidationResult result) => !result.IsValid;

    public static ModelStateDictionary ToModelState(this FluentValidation.Results.ValidationResult result)
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

    public static Result ToFailResult(this FluentValidation.Results.ValidationResult validationResult)
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
#region Controller
[ApiController]
[Route("[controller]")]
public class ContaController : ControllerBase
{
    private readonly ILogger<ContaController> _logger;
    private readonly IValidator<InclusaoContaCommand> _validator;
    private readonly IProducer _producer;
    private readonly IDataReader _dataReader;
    public ContaController(ILogger<ContaController> logger, IProducer producer, IDataReader dataWrite, IValidator<InclusaoContaCommand> validator)
    {
        _logger = logger;
        _producer = producer;
        _dataReader = dataWrite;
        _validator = validator;
    }

    [HttpPost(Name = "PostWeatherForecast")]
    public async Task<Result> Post(InclusaoContaCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);
        if (validation.IsInvalid())
            return await Task.FromResult(validation.ToFailResult());
        try
        {
            Conta entity = command.MapTo<Conta>();
            await _dataReader.Insert(entity, cancellationToken);
            ContaIncluidaEvent @event = entity.MapTo<ContaIncluidaEvent>();
            await _producer.Send(@event, cancellationToken);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }
}
#endregion