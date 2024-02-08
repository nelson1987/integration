using Adrian.Core.Events;
using FluentResults;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Adrian.Core.Producers;

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
public class ContaApiEventsProducer : Producer<AlunoCriadoEvent>
{
    public ContaApiEventsProducer(IBus producer, ILogger<Producer<AlunoCriadoEvent>> logger) : base(producer, logger)
    {
    }
}
