using FluentResults;
using FluentValidation;
using Integration.Api.Features;
using Microsoft.AspNetCore.Mvc;

namespace Integration.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ContaController : ControllerBase
{
    private readonly ILogger<ContaController> _logger;
    private readonly IValidator<InclusaoContaCommand> _validator;
    private readonly IProducer<ContaIncluidaEvent> _producer;
    private readonly IDataReader<Conta> _dataReader;
    public ContaController(ILogger<ContaController> logger,
        IProducer<ContaIncluidaEvent> producer,
    IDataReader<Conta> dataReader,
    IValidator<InclusaoContaCommand> validator)
    {
        _logger = logger;
        _producer = producer;
        _dataReader = dataReader;
        _validator = validator;
    }

    [HttpPost(Name = "PostConta")]
    public async Task<Result> Post(InclusaoContaCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);
        if (validation.IsInvalid())
            return await Task.FromResult(validation.ToFailResult());

        var entity = command.MapTo<Conta>();
        var datareader = await _dataReader.Insert(entity, cancellationToken);
        if (datareader!.IsFailed)
            return Result.Fail("Erro ao tentar persistir o dado");

        var @event = entity.MapTo<ContaIncluidaEvent>();
        var produzido = await _producer.SendAsync(@event, cancellationToken);
        if (produzido!.IsFailed)
            return Result.Fail("Erro ao tentar publicar o evento");

        return Result.Ok();
    }
}