using FluentResults;
using FluentValidation.TestHelper;
using MassTransit;

namespace Integration.Tests.Units;
public class ContaApiEventsProducerUnitTests
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
    private readonly ContaApiEventsProducer _producer;
    private readonly ContaIncluidaEvent _event;
    private readonly CancellationToken _token = CancellationToken.None;
    private readonly Mock<IBus> _bus;

    public ContaApiEventsProducerUnitTests()
    {
        _event = _fixture.Create<ContaIncluidaEvent>();

        _bus = _fixture.Freeze<Mock<IBus>>();
        _bus
             .Setup(x => x.Publish(It.IsAny<ContaIncluidaEvent>(), _token))
             .Returns(Task.CompletedTask);

        _producer = _fixture.Build<ContaApiEventsProducer>()
        .OmitAutoProperties()
        .Create();
    }

    [Fact]
    public async Task Deve_Criar_Comando_Com_SucessoAsync()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var producer = await _producer.SendAsync(_event, _token);
        // Assert
        Assert.NotNull(producer);
        Assert.True(producer.IsSuccess);
        //Assert.Equal(nomeTitular, comando.NomeTitular);
        //Assert.Equal(saldoInicial, comando.SaldoInicial);
        //Assert.Equal(ativo, comando.Ativo);
        //Assert.Equal(tipo, comando.Tipo);
    }
}

public class InclusaoContaCommandTests
{
    [Fact]
    public void Deve_Criar_Comando_Com_Sucesso()
    {
        // Arrange
        var id = Guid.NewGuid();
        var nomeTitular = "João Silva";
        var saldoInicial = 1000.00m;
        var ativo = true;
        var tipo = TipoConta.Corrente;

        // Act
        var comando = new InclusaoContaCommand(
            id,
            nomeTitular,
            saldoInicial,
            ativo,
            tipo
        );

        // Assert
        Assert.NotNull(comando);
        Assert.Equal(id, comando.Id);
        Assert.Equal(nomeTitular, comando.NomeTitular);
        Assert.Equal(saldoInicial, comando.SaldoInicial);
        Assert.Equal(ativo, comando.Ativo);
        Assert.Equal(tipo, comando.Tipo);
    }

    [Fact]
    public void Deve_Lancar_Excecao_Se_NomeTitular_For_Nulo()
    {
        // Arrange
        var id = Guid.NewGuid();
        string? nomeTitular = null;
        var saldoInicial = 1000.00m;
        var ativo = true;
        var tipo = TipoConta.Corrente;

        // Act
        var acao = () => new InclusaoContaCommand(
            id,
            nomeTitular,
            saldoInicial,
            ativo,
            tipo
        );

        // Assert
        var ex = Assert.Throws<ArgumentNullException>(acao);
        Assert.Equal("nomeTitular", ex.ParamName);
    }

    [Fact]
    public void Deve_Lancar_Excecao_Se_SaldoInicial_For_Negativo()
    {
        // Arrange
        var id = Guid.NewGuid();
        var nomeTitular = "João Silva";
        var saldoInicial = -1000.00m;
        var ativo = true;
        var tipo = TipoConta.Corrente;

        // Act
        var acao = () => new InclusaoContaCommand(
            id,
            nomeTitular,
            saldoInicial,
            ativo,
            tipo
        );

        // Assert
        var ex = Assert.Throws<ArgumentOutOfRangeException>(acao);
        Assert.Equal("saldoInicial", ex.ParamName);
    }
}

public class InclusaoContaCommandValidatorUnitTests
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
    private readonly IValidator<InclusaoContaCommand> _validator;
    private readonly InclusaoContaCommand _command;

    public InclusaoContaCommandValidatorUnitTests()
    {
        _command = _fixture.Build<InclusaoContaCommand>()
            .Create();
        _validator = _fixture.Create<InclusaoContaCommandValidator>();
    }

    [Fact]
    public void Given_a_valid_event_when_all_fields_are_valid_should_pass_validation()
        => _validator
            .TestValidate(_command)
            .ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Given_a_cancellation_request_with_invalid_asset_id_should_fail_validation()
        => _validator
            .TestValidate(_command with { NomeTitular = string.Empty })
            .ShouldHaveValidationErrorFor(x => x.NomeTitular)
            .Only();
}

public class ContaControllerUnitTests
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
    private readonly CancellationToken _token = CancellationToken.None;
    private readonly ContaController _controller;
    private readonly InclusaoContaCommand _request;
    private readonly Mock<IValidator<InclusaoContaCommand>> _validator;
    private readonly Mock<IProducer<ContaIncluidaEvent>> _producer;
    private readonly Mock<IDataReader<Conta>> _dataWriter;
    public ContaControllerUnitTests()
    {
        _request = _fixture.Build<InclusaoContaCommand>()
            //.With(x => x.OrderId, Guid.NewGuid())
            .Create();

        _dataWriter = _fixture.Freeze<Mock<IDataReader<Conta>>>();
        _dataWriter
             .Setup(x => x.Insert(It.IsAny<Conta>(), _token))
             .Returns(Task.FromResult(Result.Ok()));

        _producer = _fixture.Freeze<Mock<IProducer<ContaIncluidaEvent>>>();
        _producer
             .Setup(x => x.SendAsync(It.IsAny<ContaIncluidaEvent>(), _token))
             .Returns(Task.FromResult(Result.Ok()));

        _validator = _fixture.Freeze<Mock<IValidator<InclusaoContaCommand>>>();
        _validator
             .Setup(x => x.Validate(_request))
             .Returns(new FluentValidation.Results.ValidationResult());

        _controller = _fixture.Build<ContaController>()
            .OmitAutoProperties()
            .Create();
    }

    [Fact]
    public async Task Dado_Comando_Valido_Resultado_SucessoAsync()
    {
        //Act
        var response = await _controller.Post(_request, _token);

        //Assert
        Assert.True(response.IsSuccess);
    }

    [Fact]
    public async Task Dado_Comando_invalido_Resultado_FalhaAsync()
    {
        //Arrange
        var request = _request with { NomeTitular = string.Empty };

        _validator
             .Setup(x => x.Validate(request))
             .Returns(new FluentValidation.Results.ValidationResult(new[] {
                 new ValidationFailure("any-prop", "any-error-message") }));

        //Act
        var response = await _controller.Post(request, _token);

        //Assert
        Assert.False(response.IsSuccess);
    }

    [Fact]
    public async Task Dado_Produtor_Com_Erro_Resultado_FalhaAsync()
    {
        //Arrange
        _producer
             .Setup(x => x.SendAsync(It.IsAny<ContaIncluidaEvent>(), _token))
             .Returns(Task.FromResult(Result.Fail("error-message")));

        //Act
        var response = await _controller.Post(_request, _token);

        //Assert
        Assert.False(response.IsSuccess);
    }

    [Fact]
    public async Task Dado_Escritor_Com_Erro_Resultado_FalhaAsync()
    {
        //Arrange
        _producer
             .Setup(x => x.SendAsync(It.IsAny<ContaIncluidaEvent>(), _token))
             .Returns(Task.FromResult(Result.Fail("error-message")));

        //Act
        var response = await _controller.Post(_request, _token);

        //Assert
        Assert.False(response.IsSuccess);
    }
}
