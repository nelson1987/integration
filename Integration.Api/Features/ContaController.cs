using AutoMapper;
using FluentResults;
using FluentValidation.Results;
using MassTransit;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MongoDB.Driver;

namespace Integration.Api.Features;

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
public record InclusaoContaCommand(
    Guid Id, // Identificador único da conta (Guid)
    string NomeTitular, // Nome do titular da conta (string)
    decimal SaldoInicial, // Saldo inicial da conta (decimal)
    bool Ativo, // Indica se a conta está ativa (bool)
    TipoConta Tipo // Tipo da conta (enum)
);

public enum TipoConta
{
    Corrente,
    Poupanca,
    Salario
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
        //_logger.LogInformation($"Mensagem a ser produzida {nameof(@event)}.");
        await _producer.Publish(@event, cancellationToken);
        //_logger.LogInformation($"Mensagem produzida {nameof(@event)}.");
        return Result.Ok();

    }
}
public interface IDataReader<TEntity> where TEntity : class
{
    Task<Result> Insert(TEntity conta, CancellationToken cancellationToken);
}
public class DataReader<TEvent> : IDataReader<TEvent> where TEvent : class
{
    private readonly IMongoCollection<TEvent> _collection;
    private readonly ILogger<Producer<TEvent>> _logger;

    public DataReader(ILogger<Producer<TEvent>> logger)
    {
        _logger = logger;
        //    var mongoClient = new MongoClient(
        //bookStoreDatabaseSettings.Value.ConnectionString);

        //    var mongoDatabase = mongoClient.GetDatabase(
        //        bookStoreDatabaseSettings.Value.DatabaseName);

        //    _booksCollection = mongoDatabase.GetCollection<Pagamento>(
        //        bookStoreDatabaseSettings.Value.BooksCollectionName);
    }

    public Task<Result> Insert(TEvent conta, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Ok());
    }
}
public class ContaDataReader : DataReader<Conta>
{
    public ContaDataReader(ILogger<Producer<Conta>> logger) : base(logger)
    {
    }
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
