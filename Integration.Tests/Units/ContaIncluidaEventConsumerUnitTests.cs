using Integration.Tests.IntegrationTests;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Integration.Tests.Units;

public class ContaIncluidaEventConsumerUnitTests
{
    [Fact]
    public async Task ASampleTest()
    {

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
            {
                //x.SetKebabCaseEndpointNameFormatter();
                //x.AddConsumer<ContaIncluidaEventConsumer>();
                //x.UsingRabbitMq((ctx, cfg) =>
                //{
                //    cfg.ConfigureEndpoints(ctx);
                //});
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

        Assert.False(await harness.Published.Any<ContaIncluidaEvent>(x => x.Context.Message.Id == orderId));

        await harness.Bus.Publish(inclusaoConta, CancellationToken.None);

        Assert.True(await harness.Published.Any<ContaIncluidaEvent>(x => x.Context.Message.Id == orderId));

        var sagaHarness = harness.GetConsumerHarness<ContaIncluidaEventConsumer>();

        //Assert.True(await sagaHarness.Consumed.Any<ContaIncluidaEvent>(x => x.Context.Message.Id == orderId));

        var mensagem = await sagaHarness.Consumed.ConsumedValue<ContaIncluidaEvent>(x => x.Context.Message.Id == orderId);

        Assert.Equal(inclusaoConta.Id, mensagem.Id);
        Assert.Equal(inclusaoConta.Numero, mensagem.Numero);

        await harness.Stop();
    }
}
