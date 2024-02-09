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
        _logger.LogInformation($"Mensagem a ser persistida {nameof(RealizacaoProvaConsumer)}.");
        AlunoCompareceuEvent @event = context.Message;
        MatriculaAlunoCommand command = new MatriculaAlunoCommand(@event.Id, @event.Nome, @event.Documento);
        //using var tokenSource = ExpiringCancellationToken();
        //await _service.MatricularAsync(command, tokenSource.Token);
        //_logger.LogInformation($"Mensagem persistida {nameof(@event)}.");
        await context.Publish(new AlunoAprovadoEvent(command.Id, command.Nome, command.Documento));
    }
    private static CancellationTokenSource ExpiringCancellationToken(int msTimeout = 150)
    {
        var timeout = TimeSpan.FromMilliseconds(msTimeout);
        return new CancellationTokenSource(timeout);
    }
}
