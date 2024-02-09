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

public interface IAlunoHandler
{
    Task<Result<Aluno?>> GetAsync(BuscaAlunoQuery command, CancellationToken cancellationToken);
    Task<Result> CreateCandidateAsync(CriacaoCandidatoCommand command, CancellationToken cancellationToken);
}
public class AlunoHandler : IAlunoHandler
{
    private readonly ILogger<AlunoHandler> _logger;
    private readonly IAlunoReader _reader;
    private readonly IProducer<CriacaoCandidatoEvent> _eventsProducer;

    public AlunoHandler(ILogger<AlunoHandler> logger, 
                                IProducer<CriacaoCandidatoEvent> eventsProducer,
                                IAlunoReader reader)
    {
        _logger = logger;
        _eventsProducer = eventsProducer;
        _reader = reader;
    }

    public async Task<Result<Aluno?>> GetAsync(BuscaAlunoQuery command, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"{nameof(command)}:{command.ToJson()}");
        return Result.Ok(await _reader.GetAsync(command.Id, cancellationToken));
    }
    public async Task<Result> CreateCandidateAsync(CriacaoCandidatoCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"{nameof(command)}:{command.ToJson()}");
        var produtor = await _eventsProducer.SendAsync(new CriacaoCandidatoEvent(command.Nome, command.Documento), cancellationToken);
        return produtor.IsFailed ? Result.Fail(produtor.Errors.ToString()) : Result.Ok();
    }
}
