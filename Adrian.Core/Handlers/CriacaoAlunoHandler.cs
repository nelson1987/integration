using Adrian.Core.Commands;
using Adrian.Core.Entities;
using Adrian.Core.Events;
using Adrian.Core.Extensions;
using Adrian.Core.Producers;
using Adrian.Core.Repositories.Persistences;
using Adrian.Core.Repositories.Readers;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace Adrian.Core.Handlers;

public interface ICriacaoAlunoHandler
{
    Task<Result<List<Aluno>>> GetAsync(BuscaAlunoQuery command, CancellationToken cancellationToken);
    Task<Result> PostAsync(CriacaoAlunoCommand command, CancellationToken cancellationToken);
}
public class CriacaoAlunoHandler : ICriacaoAlunoHandler
{
    private readonly ILogger<CriacaoAlunoHandler> _logger;
    private readonly IAlunoPersistence _persistence;
    private readonly IAlunoReader _reader;
    private readonly IProducer<AlunoCriadoEvent> _eventsProducer;

    public CriacaoAlunoHandler(ILogger<CriacaoAlunoHandler> logger, 
                                IProducer<AlunoCriadoEvent> eventsProducer,
                                IAlunoPersistence persistence, 
                                IAlunoReader reader)
    {
        _logger = logger;
        _eventsProducer = eventsProducer;
        _persistence = persistence;
        _reader = reader;
    }

    public async Task<Result<Aluno?>> GetAsync(BuscaAlunoQuery command, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"{nameof(command)}:{command.ToJson()}");
        return Result.Ok(await _reader.GetAsync(command.Id, cancellationToken));
    }

    public async Task<Result> PostAsync(CriacaoAlunoCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"{nameof(command)}:{command.ToJson()}");
        var produtor = await _eventsProducer.SendAsync(new AlunoCriadoEvent(command.Id, command.Nome), cancellationToken);
        return produtor.IsFailed ? Result.Fail(produtor.Errors.ToString()) : Result.Ok();
    }
}
