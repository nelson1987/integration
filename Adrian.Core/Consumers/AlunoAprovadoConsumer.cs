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
        _logger.LogInformation($"Mensagem a ser persistida {nameof(AlunoAprovadoConsumer)}.");
        AlunoAprovadoEvent @event = context.Message;
        MatriculaAlunoCommand command = new MatriculaAlunoCommand(@event.Id, @event.Nome, @event.Documento);
        //using var tokenSource = ExpiringCancellationToken();
        //await _service.MatricularAsync(command, tokenSource.Token);
        //_logger.LogInformation($"Mensagem persistida {nameof(@event)}.");
        await context.Publish(new AlunoInscritoEvent(command.Id, command.Nome, command.Documento));
    }
    private static CancellationTokenSource ExpiringCancellationToken(int msTimeout = 150)
    {
        var timeout = TimeSpan.FromMilliseconds(msTimeout);
        return new CancellationTokenSource(timeout);
    }
}
