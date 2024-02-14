namespace Integration.Tests.Units;

public class ContaIncluidaEventConsumerUnitTests
{
    [Fact]
    public async Task ASampleTest()
    {

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
            {
                x.SetKebabCaseEndpointNameFormatter();
                x.AddConsumer<ContaIncluidaEventConsumer>();
                x.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.ConfigureEndpoints(ctx);
                });
            })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();

        await harness.Start();

        var orderId = Guid.NewGuid();

        var inclusaoConta = new ContaIncluidaEvent()
        {
            Id = orderId,
            Numero = "NomeTitular"
        };

        Assert.Equal(0, await harness.Consumed.SelectAsync<ContaIncluidaEvent>(x => x.Context.Message.Id == orderId).Count());

        Assert.False(await harness.Published.Any<ContaIncluidaEvent>(x => x.Context.Message.Id == orderId));

        await harness.Bus.Publish(inclusaoConta, CancellationToken.None);

        Assert.True(await harness.Published.Any<ContaIncluidaEvent>(x => x.Context.Message.Id == orderId));

        var sagaHarness = harness.GetConsumerHarness<ContaIncluidaEventConsumer>();

        Assert.True(await sagaHarness.Consumed.Any<ContaIncluidaEvent>(x => x.Context.Message.Id == orderId));

        Assert.Equal(1, await sagaHarness.Consumed.SelectAsync<ContaIncluidaEvent>(x => x.Context.Message.Id == orderId).Count());

        var mensagem = await sagaHarness.Consumed.ConsumedValue<ContaIncluidaEvent>();

        Assert.Equal(inclusaoConta.Id, mensagem.Id);
        Assert.Equal(inclusaoConta.Numero, mensagem.Numero);
    }
}
