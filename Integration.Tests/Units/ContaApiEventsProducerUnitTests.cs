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
