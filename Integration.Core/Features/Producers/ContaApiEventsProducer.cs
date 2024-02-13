using Integration.Api.Features;
using Integration.Core.Features.Events;

namespace Integration.Core.Features.Producers;

public class ContaApiEventsProducer : Producer<ContaIncluidaEvent>
{
    public ContaApiEventsProducer(IBus producer, ILogger<Producer<ContaIncluidaEvent>> logger) : base(producer, logger)
    {
    }
}
