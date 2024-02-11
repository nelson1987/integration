using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MongoDB.Driver;

namespace CourseTDD.Api;
public record ContaCriadaEvent(Guid Id, string Nome);
public class ContaCriadaConsumer : IConsumer<ContaCriadaEvent>
{
    private readonly ILogger<ContaCriadaConsumer> _logger;

    public ContaCriadaConsumer(ILogger<ContaCriadaConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<ContaCriadaEvent> context)
    {
        _logger.LogInformation($"Consumido: {context.Message}");
        var messageId = context.Headers.Get<Guid>("MessageId");
        return Task.CompletedTask;
    }
}
//public class BusObserver : IBusObserver
//{
//    public void CreateFaulted(Exception exception)
//    {
//        throw new NotImplementedException();
//    }

//    public void PostCreate(IBus bus)
//    {
//        throw new NotImplementedException();
//    }

//    public Task PostStart(IBus bus, Task<BusReady> busReady)
//    {
//        throw new NotImplementedException();
//    }

//    public Task PostStop(IBus bus)
//    {
//        throw new NotImplementedException();
//    }

//    public Task PreStart(IBus bus)
//    {
//        throw new NotImplementedException();
//    }

//    public Task PreStop(IBus bus)
//    {
//        throw new NotImplementedException();
//    }

//    public Task StartFaulted(IBus bus, Exception exception)
//    {
//        throw new NotImplementedException();
//    }

//    public Task StopFaulted(IBus bus, Exception exception)
//    {
//        throw new NotImplementedException();
//    }
//}
public class SendObserver : ISendObserver
{
    private readonly ILogger<ConsumeObserver> _logger;

    public SendObserver(ILogger<ConsumeObserver> logger)
    {
        _logger = logger;
    }

    public async Task PostSend<T>(SendContext<T> context) where T : class
    {
        _logger.LogInformation($"PostSend(): {context}");
        await Task.CompletedTask;
    }

    public async Task PreSend<T>(SendContext<T> context) where T : class
    {
        _logger.LogInformation($"PreSend(): {context}");
        await Task.CompletedTask;
    }

    public async Task SendFault<T>(SendContext<T> context, Exception exception) where T : class
    {
        _logger.LogInformation($"SendFault(): {context}");
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
public static class PagamentoController
{
    public enum FormaPagamento { CartaoCredito = 1 }

    public static IServiceCollection AddDependencies(this IServiceCollection services)
    {
        var settings = new
        {
            MongoClient = "mongodb://root:example@localhost:27017/",
            Database = "sales"
        };
        var mongoClient = new MongoClient(settings.MongoClient);
        services.AddSingleton(mongoClient.GetDatabase(settings.Database));
        //services.AddBusObserver<BusObserver>();
        services.AddConsumeObserver<ConsumeObserver>();
        services.AddSendObserver<SendObserver>();


        services.AddScoped<IContaService, ContaService>();
        services.AddScoped<IValidator<InclusaoContaCommand>, InclusaoContaCommandValidator>();


        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();
            x.AddConsumer<ContaCriadaConsumer>();
            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host("amqp://guest:guest@localhost:5672");
                cfg.ConfigureEndpoints(ctx);
                //cfg.UseRawJsonSerializer();
            });
        });
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
    public required string Numero { get; set; }
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
public interface IContaService
{
    Task<Result> CriarConta(InclusaoContaCommand command);
}
public class ContaService : IContaService
{
    private readonly IBus _bus;
    private readonly IValidator<InclusaoContaCommand> _validator;

    public ContaService(IValidator<InclusaoContaCommand> validator, IBus bus)
    {
        _validator = validator;
        _bus = bus;
    }

    //private readonly IValidator<InclusaoContaCommand> _validator;
    public async Task<Result> CriarConta(InclusaoContaCommand command)
    {
        var paginationHeaderValidation = _validator.Validate(command);
        if (!paginationHeaderValidation.IsValid)
            return await Task.FromResult(paginationHeaderValidation.ToFailResult());

        await _bus.Send(new ContaCriadaEvent(Guid.NewGuid(), Guid.NewGuid().ToString()));

        //Conta entity = new Conta();
        //return Task.FromResult(Results.Created($"/{entity.Id}", entity));
        return await Task.FromResult(Result.Ok());
    }
}

public interface IMongoContext
{
    public IMongoCollection<Conta> Contas { get; }
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


public class ContasController : ControllerBase
{
    private readonly ConsultaFinanceiraContext _context;

    public ContasController(ConsultaFinanceiraContext context)
    {
        _context = context;
    }

    [HttpGet("{id}/saldo")]
    public async Task<Result<decimal>> GetSaldo(Guid id, CancellationToken cancellationToken)
    {
        var filter = Builders<Conta>.Filter.Eq(x => x.Id, id);
        var conta = await _context.Contas
            .Find(filter)
            .FirstOrDefaultAsync(cancellationToken);

        return conta == null ? Result.Fail("Não encontrado") : Result.Ok(conta.Saldo);
    }

    [HttpGet("{id}/extrato")]
    public async Task<Result<List<Transacao>>> GetExtrato(Guid id, CancellationToken cancellationToken)
    {
        var filter = Builders<Conta>.Filter.Eq(x => x.Id, id);
        var conta = await _context.Contas
        .Find(filter)
            .FirstOrDefaultAsync(cancellationToken);

        return conta.Transacoes.Any() ? Result.Ok(conta.Transacoes) : Result.Fail("Não encontrado");
    }
}

