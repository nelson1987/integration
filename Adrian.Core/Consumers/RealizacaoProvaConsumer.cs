using Adrian.Core.Commands;
using Adrian.Core.Events;
using Adrian.Core.Handlers;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Adrian.Core.Consumers;

public class RealizacaoProvaConsumer : IConsumer<AlunoCompareceuEvent>
{
    private readonly ILogger<RealizacaoProvaConsumer> _logger;
    private readonly IAlunoService _service;

    public RealizacaoProvaConsumer(ILogger<RealizacaoProvaConsumer> logger, IAlunoService service)
    {
        _logger = logger;
        _service = service;
    }

    public async Task Consume(ConsumeContext<AlunoCompareceuEvent> context)
    {
        AlunoCompareceuEvent @event = context.Message;
        MatriculaAlunoCommand command = new MatriculaAlunoCommand(@event.Id, @event.Nome, @event.Documento);
        await _service.MatricularAsync(command, CancellationToken.None);
        _logger.LogInformation($"Mensagem persistida {nameof(@event)}.");
        await context.RespondAsync(new AlunoAprovadoEvent(command.Id, command.Nome, command.Documento));
    }
}
