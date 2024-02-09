using Adrian.Core.Commands;
using Adrian.Core.Events;
using Adrian.Core.Handlers;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Adrian.Core.Consumers;

public class AlunoAprovadoConsumer : IConsumer<AlunoAprovadoEvent>
{
    private readonly ILogger<AlunoAprovadoConsumer> _logger;
    private readonly IAlunoService _service;

    public AlunoAprovadoConsumer(ILogger<AlunoAprovadoConsumer> logger, IAlunoService service)
    {
        _logger = logger;
        _service = service;
    }

    public async Task Consume(ConsumeContext<AlunoAprovadoEvent> context)
    {
        AlunoAprovadoEvent @event = context.Message;
        MatriculaAlunoCommand command = new MatriculaAlunoCommand(@event.Id, @event.Nome, @event.Documento);
        await _service.MatricularAsync(command, CancellationToken.None);
        _logger.LogInformation($"Mensagem persistida {nameof(@event)}.");
        await context.RespondAsync(new AlunoInscritoEvent(command.Id, command.Nome, command.Documento));
    }
}
