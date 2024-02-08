using Adrian.Core.Entities;
using Adrian.Core.Events;
using Adrian.Core.Repositories.Persistences;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Adrian.Core.Consumers;
public class AlunoCriadoConsumer : IConsumer<AlunoCriadoEvent>
{
    private readonly ILogger<AlunoCriadoConsumer> _logger;
    private readonly IAlunoPersistence _persistence;

    public AlunoCriadoConsumer(ILogger<AlunoCriadoConsumer> logger, IAlunoPersistence persistence)
    {
        _logger = logger;
        _persistence = persistence;
    }

    public async Task Consume(ConsumeContext<AlunoCriadoEvent> context)
    {
        AlunoCriadoEvent @event = context.Message;
        Aluno entidade = new Aluno()
        {
            Nome = @event.Nome
        };
        _logger.LogInformation($"Mensagem a ser persistida {nameof(@event)}.");
        await _persistence.Insert(entidade, CancellationToken.None);
        _logger.LogInformation($"Mensagem persistida {nameof(@event)}.");

        //await context.RespondAsync<AlunoPersistidoEvent>(new AlunoPersistidoEvent());
    }
}