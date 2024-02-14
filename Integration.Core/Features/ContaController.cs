using FluentResults;
using Integration.Core.Features.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MongoDB.Driver;

namespace Integration.Api.Features;

#region Core
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
        //_logger.LogInformation($"Mensagem a ser produzida @event.");
        await _producer.Publish(@event, cancellationToken);
        //_logger.LogInformation($"Mensagem produzida @event.");
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
