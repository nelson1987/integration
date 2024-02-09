using Adrian.Core.Commands;
using Adrian.Core.Events;
using Adrian.Core.Handlers;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Adrian.Core.Consumers;

public class InscricaoTurmaConsumer : IConsumer<AlunoInscritoEvent>
{
    private readonly ILogger<InscricaoTurmaConsumer> _logger;
    private readonly IAlunoService _service;

    public InscricaoTurmaConsumer(ILogger<InscricaoTurmaConsumer> logger, IAlunoService service)
    {
        _logger = logger;
        _service = service;
    }

    public async Task Consume(ConsumeContext<AlunoInscritoEvent> context)
    {
        AlunoInscritoEvent @event = context.Message;
        MatriculaAlunoCommand command = new MatriculaAlunoCommand(@event.Id, @event.Nome, @event.Documento);
        await _service.MatricularAsync(command, CancellationToken.None);
        _logger.LogInformation($"Mensagem persistida {nameof(@event)}.");
        await context.RespondAsync(new AlunoTurmaInseridaEvent(command.Id, command.Nome, command.Documento));
    }
}
