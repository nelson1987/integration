using Integration.Api;
using MassTransit;
using MassTransit.Testing;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;

namespace Integration.Tests.IntegrationTests;

public class ContaIncluidaEventConsumerIntegrationTests
{
    [Fact]
    public async Task Should_have_the_submitted_status()
    {

        await using var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder
                .ConfigureServices(services =>
                {
                    services.AddMassTransitTestHarness(x =>
                    {
                        x.SetKebabCaseEndpointNameFormatter();
                        x.AddConsumer<ContaIncluidaEventConsumer>();
                        x.UsingRabbitMq((ctx, cfg) =>
                        {
                            cfg.ConfigureEndpoints(ctx);
                        });
                    });
                }));
        var testHarness = application.Services.GetTestHarness();
        using var client = application.CreateClient();
        const string submitOrderUrl = "/conta";
        var orderId = Guid.NewGuid();
        var inclusaoConta = new InclusaoContaCommand(
    orderId,
    "NomeTitular",
    10.00M,
    true,
    TipoConta.Corrente);

        Assert.Equal(0, await testHarness.Consumed.SelectAsync<ContaIncluidaEvent>(x => x.Context.Message.Id == orderId).Count());

        var submiteOrderResponse = await client.PostAsJsonAsync(submitOrderUrl, inclusaoConta, CancellationToken.None);

        submiteOrderResponse.EnsureSuccessStatusCode();

        Assert.True(await testHarness.Published.Any<ContaIncluidaEvent>(x => x.Context.Message.Id == orderId));

        var sagatestharness = testHarness.GetConsumerHarness<ContaIncluidaEventConsumer>();

        Assert.True(await sagatestharness.Consumed.Any<ContaIncluidaEvent>(x => x.Context.Message.Id == orderId));

        Assert.Equal(1, await sagatestharness.Consumed.SelectAsync<ContaIncluidaEvent>(x => x.Context.Message.Id == orderId).Count());

        var mensagem = await sagatestharness.Consumed.ConsumedValue<ContaIncluidaEvent>();

        Assert.Equal(orderId, mensagem.Id);
    }
}
public static class MassTransitExtensions
{
    public static async Task<TEvent> ConsumedValue<TEvent>(this IReceivedMessageList received) where TEvent : class
    {
        var mensagem = await received.SelectAsync<TEvent>().FirstOrDefault();
        return mensagem.Context.Message;
    }
}
