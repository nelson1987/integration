using FluentResults;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Adrian.Core;
public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public record CriacaoAlunoCommand();

public record AlunoCriadoEvent(Guid Id, string Nome);
public interface IAlunoApiEventsConsumer { }
public class AlunoApiEventsConsumer : IAlunoApiEventsConsumer { }
public class AlunoConsumer : IConsumer<AlunoCriadoEvent>
{
    public Task Consume(ConsumeContext<AlunoCriadoEvent> context)
    {
        throw new NotImplementedException();
    }
}
#region
public interface IAlunoPersistence { }
public class AlunoPersistence : IAlunoPersistence { }
public interface IAlunoReader { }
public class AlunoReader : IAlunoReader { }
public interface IContaApiEventsProducer
{
    Task<Result> SendAlunoCriado(AlunoCriadoEvent @event, CancellationToken cancellationToken);
}

public class ContaApiEventsProducer : IContaApiEventsProducer
{
    private readonly IBus _producer;
    private readonly ILogger<ContaApiEventsProducer> _logger;

    public ContaApiEventsProducer(IBus producer, ILogger<ContaApiEventsProducer> logger)
    {
        _producer = producer;
        _logger = logger;
    }

    public async Task<Result> SendAlunoCriado(AlunoCriadoEvent @event, CancellationToken cancellationToken)
    {
        await _producer.Publish(@event, cancellationToken);
        _logger.LogInformation($"Mensagem Produzida {nameof(@event)}.");
        //return Result.Ok();
        //const string topic = EventsTopics.AlunoCriadoRequested.Name;
        //const string subject = nameof(EventsTopics.AlunoCriadoRequested.Subjects.AlunoCriadoRequested);
        //var produceResult = await _eventCommunication.ProduceEvent(@event, topic, subject, cancellationToken);
        //return produceResult.IsFailed ? Result.Fail("Failed to send orders create event") :
        return Result.Ok();
    }


}

public interface ICriacaoAlunoHandler
{
    Task<Result> Handle(CriacaoAlunoCommand command, CancellationToken cancellationToken);
}

public class CriacaoAlunoHandler : ICriacaoAlunoHandler
{
    private readonly IAlunoPersistence _persistence;
    private readonly IAlunoReader _reader;
    private readonly IContaApiEventsProducer _eventsProducer;

    public CriacaoAlunoHandler(IAlunoPersistence persistence, IAlunoReader reader, IContaApiEventsProducer eventsProducer)
    {
        _persistence = persistence;
        _reader = reader;
        _eventsProducer = eventsProducer;
    }

    public async Task<Result> Handle(CriacaoAlunoCommand command, CancellationToken cancellationToken)
    {
        var produtor = await _eventsProducer.SendAlunoCriado(new AlunoCriadoEvent(Guid.NewGuid(), "teste"), cancellationToken);
        return produtor.IsFailed ? Result.Fail(produtor.Errors.ToString()) : Result.Ok();
    }
}
#endregion
public static class Dependencies
{
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        services.AddScoped<ICriacaoAlunoHandler, CriacaoAlunoHandler>();
        services.AddScoped<IAlunoPersistence, AlunoPersistence>();
        services.AddScoped<IAlunoReader, AlunoReader>();
        services.AddScoped<IContaApiEventsProducer, ContaApiEventsProducer>();
        services.AddScoped<IAlunoApiEventsConsumer, AlunoApiEventsConsumer>();
        return services;
    }
    public static IServiceCollection AddMassTransit(this IServiceCollection services)
    //, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();
            x.AddConsumer<AlunoConsumer>();
            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host("amqp://guest:guest@localhost:5672");
                cfg.ConfigureEndpoints(ctx);
                //cfg.UseRawJsonSerializer();
            });
        });
        return services;
    }
}