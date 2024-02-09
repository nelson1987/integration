﻿using Adrian.Core.Entities;
using Adrian.Core.Events;
using Adrian.Core.Extensions;
using Adrian.Core.Repositories.Persistences;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Adrian.Core.Consumers;
public class CriacaoCandidatoConsumer : IConsumer<CriacaoCandidatoEvent>
{
    private readonly ILogger<CriacaoCandidatoConsumer> _logger;
    private readonly IAlunoPersistence _persistence;

    public CriacaoCandidatoConsumer(ILogger<CriacaoCandidatoConsumer> logger, IAlunoPersistence persistence)
    {
        _logger = logger;
        _persistence = persistence;
    }

    public async Task Consume(ConsumeContext<CriacaoCandidatoEvent> context)
    {
        CriacaoCandidatoEvent @event = context.Message;
        Aluno entidade = new Aluno()
        {
            Id = Guid.NewGuid(),
            Nome = @event.Nome,
            Documento = @event.Documento,
            Status = StatusAluno.Criado
        };
        _logger.LogInformation($"Mensagem a ser persistida {nameof(@event)}.{@event.ToJson()}");
        await _persistence.CreateAsync(entidade, CancellationToken.None);
        _logger.LogInformation($"Mensagem persistida {nameof(@event)}.");

        await context.RespondAsync(new AlunoMatriculadoEvent(entidade.Id, entidade.Nome, entidade.Documento));
    }
}