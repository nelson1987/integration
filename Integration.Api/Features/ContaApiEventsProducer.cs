using MassTransit;

namespace Integration.Api.Features;

public class ContaApiEventsProducer : Producer<ContaIncluidaEvent>
{
    public ContaApiEventsProducer(IBus producer, ILogger<Producer<ContaIncluidaEvent>> logger) : base(producer, logger)
    {
    }
}
