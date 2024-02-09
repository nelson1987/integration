using Adrian.Core.Commands;
using Adrian.Core.Entities;
using Adrian.Core.Extensions;
using Adrian.Core.Repositories;
using Adrian.Core.Repositories.Persistences;
using Adrian.Core.Repositories.Readers;
using Microsoft.Extensions.Logging;

namespace Adrian.Core.Handlers;

public interface IAlunoService
{
    Task MatricularAsync(MatriculaAlunoCommand command, CancellationToken cancellationToken);
    Task RealizarProvaAsync(RealizaProvaCommand command, CancellationToken cancellationToken);
    Task PassarProvaAsync(PassaProvaCommand command, CancellationToken cancellationToken);
    Task InscreverTurmaAsync(InscricaoTurmaCommand command, CancellationToken cancellationToken);
    Task EscolherMateriaAsync(EscolheMateriaCommand command, CancellationToken cancellationToken);
}
public class AlunoService : IAlunoService
{
    private readonly ILogger<AlunoService> _logger;
    private readonly IAlunoPersistence _persistence;
    private readonly IAlunoReader _reader;
    private readonly IUnitOfWork _unitOfWork;

    public AlunoService(ILogger<AlunoService> logger,
                                IAlunoPersistence persistence,
                                IAlunoReader reader,
                                IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _persistence = persistence;
        _reader = reader;
        _unitOfWork = unitOfWork;
    }

    public async Task EscolherMateriaAsync(EscolheMateriaCommand command, CancellationToken cancellationToken)
    {
        await using var session = await _unitOfWork.StartSession(cancellationToken);
        session.StartTransaction();
        _logger.LogInformation($"{nameof(command)}:{command.ToJson()}");
        Aluno entidade = await _reader.GetAsync(command.Id, cancellationToken);
        _logger.LogInformation($"Mensagem a ser persistida {nameof(entidade)}.");
        await _persistence.UpdateAsync(entidade, StatusAluno.MatriculaMateria, CancellationToken.None);

        _logger.LogInformation($"Mensagem persistida {nameof(entidade)}.");
        await session.CommitTransaction(cancellationToken);
    }

    public async Task InscreverTurmaAsync(InscricaoTurmaCommand command, CancellationToken cancellationToken)
    {
        await using var session = await _unitOfWork.StartSession(cancellationToken);
        session.StartTransaction();
        _logger.LogInformation($"{nameof(command)}:{command.ToJson()}");
        Aluno entidade = await _reader.GetAsync(command.Id, cancellationToken);
        _logger.LogInformation($"Mensagem a ser persistida {nameof(entidade)}.");
        await _persistence.UpdateAsync(entidade, StatusAluno.Inscrito, CancellationToken.None);
        _logger.LogInformation($"Mensagem persistida {nameof(entidade)}.");
        await session.CommitTransaction(cancellationToken);
    }

    public async Task MatricularAsync(MatriculaAlunoCommand command, CancellationToken cancellationToken)
    {
        await using var session = await _unitOfWork.StartSession(cancellationToken);
        session.StartTransaction();
        _logger.LogInformation($"{nameof(command)}:{command.ToJson()}");
        var entidade = await _reader.GetAsync(command.Id,cancellationToken);
        _logger.LogInformation($"Mensagem a ser persistida {nameof(entidade)}.");
        await _persistence.UpdateAsync(entidade!, StatusAluno.Matriculado, CancellationToken.None);
        _logger.LogInformation($"Mensagem persistida {nameof(entidade)}.");
        await session.CommitTransaction(cancellationToken);
    }

    public async Task PassarProvaAsync(PassaProvaCommand command, CancellationToken cancellationToken)
    {
        await using var session = await _unitOfWork.StartSession(cancellationToken);
        session.StartTransaction();
        _logger.LogInformation($"{nameof(command)}:{command.ToJson()}");
        Aluno entidade = await _reader.GetAsync(command.Id, cancellationToken);
        _logger.LogInformation($"Mensagem a ser persistida {nameof(entidade)}.");
        await _persistence.UpdateAsync(entidade, StatusAluno.Aprovado, CancellationToken.None);
        _logger.LogInformation($"Mensagem persistida {nameof(entidade)}.");
        await session.CommitTransaction(cancellationToken);
    }

    public async Task RealizarProvaAsync(RealizaProvaCommand command, CancellationToken cancellationToken)
    {
        await using var session = await _unitOfWork.StartSession(cancellationToken);
        session.StartTransaction();
        _logger.LogInformation($"{nameof(command)}:{command.ToJson()}");
        Aluno entidade = await _reader.GetAsync(command.Id, cancellationToken);
        _logger.LogInformation($"Mensagem a ser persistida {nameof(entidade)}.");
        await _persistence.UpdateAsync(entidade, StatusAluno.ProvaRealizada, CancellationToken.None);
        _logger.LogInformation($"Mensagem persistida {nameof(entidade)}.");
        await session.CommitTransaction(cancellationToken);
    }
}
