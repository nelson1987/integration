using Adrian.Core.Commands;
using Adrian.Core.Events;
using Adrian.Core.Handlers;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Adrian.Core.Consumers;

public class MatriculaAlunoConsumer : IConsumer<AlunoMatriculadoEvent>
{
    private readonly ILogger<MatriculaAlunoConsumer> _logger;
    private readonly IAlunoService _service;

    public MatriculaAlunoConsumer(ILogger<MatriculaAlunoConsumer> logger, IAlunoService service)
    {
        _logger = logger;
        _service = service;
    }

    public async Task Consume(ConsumeContext<AlunoMatriculadoEvent> context)
    {
        AlunoMatriculadoEvent @event = context.Message;
        _logger.LogInformation($"Mensagem a ser persistida {nameof(@event)}.");
        MatriculaAlunoCommand command = new MatriculaAlunoCommand(@event.Id, @event.Nome, @event.Documento);
        await _service.MatricularAsync(command, CancellationToken.None);
        _logger.LogInformation($"Mensagem persistida {nameof(@event)}.");
        await context.RespondAsync(new AlunoCompareceuEvent(command.Id, command.Nome, command.Documento));
    }
}
