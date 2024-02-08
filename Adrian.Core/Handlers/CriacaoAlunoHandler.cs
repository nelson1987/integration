using Adrian.Core.Commands;
using Adrian.Core.Events;
using Adrian.Core.Producers;
using Adrian.Core.Repositories.Persistences;
using Adrian.Core.Repositories.Readers;
using FluentResults;

namespace Adrian.Core.Handlers;

public interface ICriacaoAlunoHandler
{
    Task<Result> Handle(CriacaoAlunoCommand command, CancellationToken cancellationToken);
}
public class CriacaoAlunoHandler : ICriacaoAlunoHandler
{
    private readonly IAlunoPersistence _persistence;
    private readonly IAlunoReader _reader;
    private readonly IProducer<AlunoCriadoEvent> _eventsProducer;

    public CriacaoAlunoHandler(IAlunoPersistence persistence, IAlunoReader reader, IProducer<AlunoCriadoEvent> eventsProducer)
    {
        _persistence = persistence;
        _reader = reader;
        _eventsProducer = eventsProducer;
    }

    public async Task<Result> Handle(CriacaoAlunoCommand command, CancellationToken cancellationToken)
    {
        var produtor = await _eventsProducer.Send(new AlunoCriadoEvent(command.Id, command.Nome), cancellationToken);
        return produtor.IsFailed ? Result.Fail(produtor.Errors.ToString()) : Result.Ok();
    }
}
