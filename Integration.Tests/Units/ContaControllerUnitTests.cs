using FluentResults;

namespace Integration.Tests.Units;

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
