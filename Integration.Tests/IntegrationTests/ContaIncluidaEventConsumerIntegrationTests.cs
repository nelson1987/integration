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

                        x.AddConsumeObserver<ConsumeObserver>();
                        x.AddPublishObserver<PublishObserver>();

                        x.SetKebabCaseEndpointNameFormatter();
                        x.AddConsumer<ContaIncluidaEventConsumer>();
                        x.UsingRabbitMq((ctx, cfg) =>
                        {
                            //cfg.Host("amqp://guest:guest@localhost:5672");
                            cfg.ConfigureEndpoints(ctx);
                            //cfg.UseRawJsonSerializer();
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

        var mensgaem = await sagatestharness.Consumed.SelectAsync<ContaIncluidaEvent>(x => x.Context.Message.Id == orderId).FirstOrDefault();

        Assert.Equal(orderId, mensgaem.Context.Message.Id);
    }
}
