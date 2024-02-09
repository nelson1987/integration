using Adrian.Core.Commands;
using Adrian.Core.Events;
using Adrian.Core.Handlers;
using Adrian.Core.Repositories;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Adrian.Core.Consumers;

public class MatriculaAlunoConsumer : IConsumer<AlunoMatriculadoEvent>
{
    private readonly ILogger<MatriculaAlunoConsumer> _logger;
    private readonly IAlunoService _service;
    private readonly IUnitOfWork _unitOfWork;

    public MatriculaAlunoConsumer(ILogger<MatriculaAlunoConsumer> logger, IAlunoService service, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _service = service;
        _unitOfWork = unitOfWork;
    }

    public async Task Consume(ConsumeContext<AlunoMatriculadoEvent> context)
    {
        AlunoMatriculadoEvent @event = context.Message;
        _logger.LogInformation($"Mensagem a ser persistida {nameof(@event)}.");
        MatriculaAlunoCommand command = new MatriculaAlunoCommand(@event.Id, @event.Nome, @event.Documento);
        using var tokenSource = ExpiringCancellationToken();
        await _service.MatricularAsync(command, CancellationToken.None);
        _logger.LogInformation($"Mensagem persistida {nameof(@event)}.");
        await context.Publish(new AlunoCompareceuEvent(command.Id, command.Nome, command.Documento));
    }
    private static CancellationTokenSource ExpiringCancellationToken(int msTimeout = 150)
    {
        var timeout = TimeSpan.FromMilliseconds(msTimeout);
        return new CancellationTokenSource(timeout);
    }
}
