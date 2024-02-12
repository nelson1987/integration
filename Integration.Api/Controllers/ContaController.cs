using AutoMapper;
using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MongoDB.Driver;

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
        services.AddScoped<IProducer<ContaIncluidaEvent>, ContaApiEventsProducer>();
        return services;
    }

    private static IServiceCollection AddRabbitMq(this IServiceCollection services)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumeObserver<ConsumeObserver>();
            x.AddPublishObserver<PublishObserver>();

            x.SetKebabCaseEndpointNameFormatter();
            x.AddConsumer<ContaIncluidaEventConsumer>();
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
public class PublishObserver : IPublishObserver
{
    private readonly ILogger<PublishObserver> _logger;

    public PublishObserver(ILogger<PublishObserver> logger)
    {
        _logger = logger;
    }

    public async Task PostPublish<T>(PublishContext<T> context) where T : class
    {
        _logger.LogInformation($"PostPublish(): {context}");
        await Task.CompletedTask;
    }

    public async Task PrePublish<T>(PublishContext<T> context) where T : class
    {
        _logger.LogInformation($"PrePublish(): {context}");
        await Task.CompletedTask;
    }

    public async Task PublishFault<T>(PublishContext<T> context, Exception exception) where T : class
    {
        _logger.LogInformation($"PublishFault(): {context}");
        await Task.CompletedTask;
    }
}
public class ConsumeObserver : IConsumeObserver
{
    private readonly ILogger<ConsumeObserver> _logger;

    public ConsumeObserver(ILogger<ConsumeObserver> logger)
    {
        _logger = logger;
    }

    public async Task ConsumeFault<T>(ConsumeContext<T> context, Exception exception) where T : class
    {
        _logger.LogInformation($"ConsumeFault(): {context}");
        await Task.CompletedTask;
    }

    public async Task PostConsume<T>(ConsumeContext<T> context) where T : class
    {
        _logger.LogInformation($"PostConsume(): {context}");
        await Task.CompletedTask;
    }

    public async Task PreConsume<T>(ConsumeContext<T> context) where T : class
    {
        _logger.LogInformation($"PreConsume(): {context}");
        await Task.CompletedTask;
    }
}
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
public interface IProducer<TEvent> where TEvent : class
{
    Task<Result> SendAsync(TEvent @event, CancellationToken cancellationToken);
}
public class Producer<TEvent> : IProducer<TEvent> where TEvent : class
{
    private readonly IBus _producer;
    private readonly ILogger<Producer<TEvent>> _logger;

    public Producer(IBus producer, ILogger<Producer<TEvent>> logger)
    {
        _producer = producer;
        _logger = logger;
    }

    public async Task<Result> SendAsync(TEvent @event, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Mensagem a ser produzida {nameof(@event)}.");
        await _producer.Publish(@event, cancellationToken);
        _logger.LogInformation($"Mensagem produzida {nameof(@event)}.");
        return Result.Ok();

    }
}
public class ContaApiEventsProducer : Producer<ContaIncluidaEvent>
{
    public ContaApiEventsProducer(IBus producer, ILogger<Producer<ContaIncluidaEvent>> logger) : base(producer, logger)
    {
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
    private readonly IProducer<ContaIncluidaEvent> _producer;
    //private readonly IDataReader _dataReader;
    public ContaController(ILogger<ContaController> logger, IProducer<ContaIncluidaEvent> producer,
    //IDataReader dataWrite, 
    IValidator<InclusaoContaCommand> validator)
    {
        _logger = logger;
        _producer = producer;
        //_dataReader = dataWrite;
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
            //await _dataReader.Insert(entity, cancellationToken);
            ContaIncluidaEvent @event = entity.MapTo<ContaIncluidaEvent>();
            await _producer.SendAsync(@event, cancellationToken);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }
}
public class ContaIncluidaEventConsumer : IConsumer<ContaIncluidaEvent>
{
    private readonly ILogger<ContaIncluidaEventConsumer> _logger;

    public ContaIncluidaEventConsumer(ILogger<ContaIncluidaEventConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<ContaIncluidaEvent> context)
    {
        _logger.LogInformation($"Mensagem a ser produzida {nameof(context)}.");
        ContaIncluidaEvent @event = context.Message;
        _logger.LogInformation($"Mensagem produzida {nameof(context)}.");
        return Task.CompletedTask;
    }

}
#endregion