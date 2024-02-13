using Integration.Core.Features.Events;

namespace Integration.Core.Features.Consumers;
#region Controller
public class ContaIncluidaEventConsumer : IConsumer<ContaIncluidaEvent>
{
    private readonly ILogger<ContaIncluidaEventConsumer> _logger;
    public ContaIncluidaEventConsumer()
    {

    }
    protected ContaIncluidaEventConsumer(ILogger<ContaIncluidaEventConsumer> logger) : this()
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<ContaIncluidaEvent> context)
    {
        _logger.LogInformation($"Mensagem a ser produzida {nameof(ContaIncluidaEvent)}.");
        ContaIncluidaEvent @event = context.Message;
        _logger.LogInformation($"Mensagem produzida {nameof(ContaIncluidaEvent)}.");
        return Task.CompletedTask;
    }

}
#endregion