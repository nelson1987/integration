using AutoFixture;
using AutoFixture.AutoMoq;
using CourseTDD.Api;
using FluentAssertions;
using FluentResults.Extensions.FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;

namespace Course.Tests;

public class IntegrationApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public IntegrationApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }


    [Fact]
    public async Task Dado_Request_Valido_Deve_Retornar_CreatedAsync()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Post, "/");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
        response.Should().BeSuccessful();

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNull();
    }
}


public class UnitCommandValidatorTests
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
    private readonly IValidator<InclusaoContaCommand> _validator;
    private readonly InclusaoContaCommand _command;

    public UnitCommandValidatorTests()
    {
        _command = _fixture.Build<InclusaoContaCommand>()
            //.With(x => x.OrderId, Guid.NewGuid())
            //.With(x => x.Asset, new AssetRequest(Guid.NewGuid(), "Loan", AssetType.Current, "Class"))
            //.With(x => x.Product, Products.BankLoan)
            //.With(x => x.OrderType, OrderType.Issue)
            //.With(x => x.Reason, new ReasonRequest(Reason.InvalidBooking, "InvalidBookingDescription"))
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
            .TestValidate(_command with { Nome = string.Empty })
            .ShouldHaveValidationErrorFor(x => x.Nome)
            .Only();
}
public class UnitServicesTests
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
    private readonly ContaService _contaService;
    private readonly InclusaoContaCommand _request;
    private readonly Mock<IValidator<InclusaoContaCommand>> _validatorMock;
    private readonly CancellationToken _token = CancellationToken.None;

    public UnitServicesTests()
    {
        _request = _fixture.Build<InclusaoContaCommand>()
            //.With(x => x.Nome, Guid.NewGuid())
            .Create();

        _validatorMock = _fixture.Freeze<Mock<IValidator<InclusaoContaCommand>>>();
        _validatorMock
             .Setup(x => x.Validate(_request))
             .Returns(new FluentValidation.Results.ValidationResult());

        //var conta = _fixture.Build<Conta>()
        //    //.With(x=>x.Id, Guid.NewGuid())
        //    .Create();

        //_fixture.Freeze<>

        _contaService = _fixture.Create<ContaService>();
    }

    [Fact]
    public async Task Dado_Request_Valido_Deve_Retornar_CreatedAsync()
    {
        // Act
        var service = await _contaService.CriarConta(_request);

        // Assert
        service.Should().BeSuccess();
    }

    [Fact]
    public async Task Dado_Request_invalido_Deve_Retornar_Exception()
    {
        // Arrange
        _validatorMock
                .Setup(x => x.Validate(It.IsAny<InclusaoContaCommand>()))
                .Returns(new FluentValidation.Results.ValidationResult(new[] { new ValidationFailure("any-prop", "any-error-message") }));

        // Act
        var service = await _contaService.CriarConta(_request);

        // Assert
        service.Should().BeFailure().And.HaveError("any-error-message");
        _validatorMock.Verify(x => x.Validate(_request), Times.Once);
        //_validatorMock.VerifyNoOtherCalls();

    }
}

public class ContasControllerTests
{
    private readonly ConsultaFinanceiraContext _context;
    private readonly ContasController _controller;

    public ContasControllerTests()
    {
        _context = new ConsultaFinanceiraContext("mongodb://root:example@localhost:27017/", "sales");

        _controller = new ContasController(_context);
    }
    [Fact]
    public async Task GetSaldo_DeveRetornarSaldoCorreto()
    {
        // Arrange
        var conta = new Conta
        {
            Id = 1,
            Numero = "123456",
            Saldo = 1000.00m,
            Titular = "João Silva",
            Transacoes = new List<Transacao>()
        };

        await _context.Contas.InsertOneAsync(conta);

        // Act
        using var tokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(150));
        var result = await _controller.GetSaldo(1, tokenSource.Token);

        // Assert
        result.Should().BeSuccess();

        var saldo = (decimal)result.Value;

        saldo.Should().Be(1000.00m);
    }

    [Fact]
    public async Task GetExtrato_DeveRetornarExtratoCorreto()
    {
        // Arrange
        var conta = new Conta
        {
            Id = 1,
            Numero = "123456",
            Saldo = 1000.00m,
            Titular = "João Silva",
            Transacoes = new List<Transacao>()
        };

        var transacao1 = new Transacao
        {
            Id = 1,
            Data = DateTime.Now,
            Valor = 100.00m,
            Tipo = "Debito",
            Descricao = "Compra na loja X"
        };

        var transacao2 = new Transacao
        {
            Id = 2,
            Data = DateTime.Now.AddDays(-1),
            Valor = 50.00m,
            Tipo = "Credito",
            Descricao = "Transferência recebida"
        };

        conta.Transacoes.Add(transacao1);
        conta.Transacoes.Add(transacao2);

        await _context.Contas.InsertOneAsync(conta);

        // Act
        using var tokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(150));
        var result = await _controller.GetExtrato(1, tokenSource.Token);

        // Assert
        result.Should().BeSuccess();

        var extrato = result.Value;

        extrato.Should().HaveCount(2);
        extrato.Should().Contain(transacao1);
        extrato.Should().Contain(transacao2);
    }

}