using AutoFixture;
using AutoFixture.AutoMoq;
using FluentValidation;
using FluentValidation.Results;
using FluentValidation.TestHelper;
using Integration.Api.Controllers;
using Moq;

namespace Integration.Tests;

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
            .TestValidate(_command with { NumeroContaDebitada = string.Empty })
            .ShouldHaveValidationErrorFor(x => x.NumeroContaDebitada)
            .Only();
}

public class ContaControllerUnitTests
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
    private readonly CancellationToken _token = CancellationToken.None;
    private readonly ContaController _controller;
    private readonly InclusaoContaCommand _request;
    private readonly Mock<IValidator<InclusaoContaCommand>> _validator;
    private readonly Mock<IProducer> _producer;
    private readonly Mock<IDataReader> _dataWriter;
    public ContaControllerUnitTests()
    {
        _request = _fixture.Build<InclusaoContaCommand>()
            //.With(x => x.OrderId, Guid.NewGuid())
            .Create();

        _producer = _fixture.Freeze<Mock<IProducer>>();
        _producer
             .Setup(x => x.Send(It.IsAny<ContaIncluidaEvent>(), _token))
             .Returns(Task.CompletedTask);

        _dataWriter = _fixture.Freeze<Mock<IDataReader>>();
        _dataWriter
             .Setup(x => x.Insert(It.IsAny<Conta>(), _token))
             .Returns(Task.CompletedTask);

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
        //Arrange
        //Act
        var response = await _controller.Post(_request, _token);
        //Assert
        Assert.True(response.IsSuccess);
    }

    [Fact]
    public async Task Dado_Comando_invalido_Resultado_FalhaAsync()
    {
        //Arrange
        var request = _request with { NumeroContaDebitada = string.Empty };

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
             .Setup(x => x.Send(It.IsAny<ContaIncluidaEvent>(), _token))
             .Returns(Task.FromResult(new NotImplementedException()));
        //Act
        var response = await _controller.Post(_request, _token);
        //Assert
        Assert.False(response.IsSuccess);
    }

    [Fact]
    public async Task Dado_Escritor_Com_Erro_Resultado_FalhaAsync()
    {
        //Arrange
        _dataWriter
             .Setup(x => x.Insert(It.IsAny<Conta>(), _token))
             .Returns(Task.FromResult(new NotImplementedException()));
        //Act
        var response = await _controller.Post(_request, _token);
        //Assert
        Assert.False(response.IsSuccess);
    }
}

public class ObjectMapperTests
{
    [Fact]
    public void ValidateMappingConfigurationTest()
    {
        var mapper = ObjectMapper.Mapper;

        mapper.ConfigurationProvider.AssertConfigurationIsValid();
    }
}