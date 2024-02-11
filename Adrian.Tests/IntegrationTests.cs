using Adrian.Core.Commands;
using Adrian.Core.Consumers;
using Adrian.Core.Events;
using Adrian.Core.Producers;
using MassTransit;
using MassTransit.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;

namespace Adrian.Tests;
public class Api : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
        => builder.UseEnvironment("Test")
               .ConfigureTestServices(services =>
               {
                   services.AddMassTransitTestHarness(x =>
                   {
                       x.AddDelayedMessageScheduler();
                       x.AddConsumer<CriacaoCandidatoConsumer>();
                       //x.UsingInMemory((context, cfg) =>
                       //{
                       //    cfg.UseDelayedMessageScheduler();

                       //    cfg.ConfigureEndpoints(context);
                       //});
                   });
               });
}

public class IntegrationTest
{
    public HttpClient HttpClient;
    public Api Api;
    public ITestHarness TestHarness;
    public IProducer<CriacaoCandidatoEvent> Producer;
    public IntegrationTest()
    {
        Api = new Api();
        HttpClient = Api.CreateClient();
        TestHarness = Api.Services.GetTestHarness();
        Producer = Api.Services.GetRequiredService<IProducer<CriacaoCandidatoEvent>>();
    }
}

public class IntegrationTests : IntegrationTest
{
    public IntegrationTests() : base()
    {
    }

    [Fact]
    public async Task Integration_Api()
    {
        var order = new CriacaoCandidatoCommand("nome", "documento");
        var submitOrderResponse = await HttpClient.GetAsync($"/weatherforecast?nome={order.Nome}");
        submitOrderResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Integration_To_PubSub()
    {
        var documento = NewId.NextGuid().ToString();

        var submitOrderResponse = await HttpClient.PostAsync("/weatherforecast",
            JsonContent.Create(new CriacaoCandidatoCommand("nome", documento)));
        submitOrderResponse.EnsureSuccessStatusCode();

        var sagaTestHarness = TestHarness.GetConsumerHarness<CriacaoCandidatoConsumer>();

        Assert.True(await TestHarness.Published.Any<CriacaoCandidatoEvent>());
        Assert.True(await sagaTestHarness.Consumed.Any<CriacaoCandidatoEvent>(x => x.Context.Message.Documento == documento));
    }

    [Fact]
    public async Task Integration_PubSub()
    {
        var documento = NewId.NextGuid().ToString();

        CriacaoCandidatoEvent @event = new CriacaoCandidatoEvent("nome", documento);

        using var tokenSource = ExpiringCancellationToken();
        await Producer.SendAsync(@event, tokenSource.Token);
        Assert.True(await TestHarness.Published.Any<CriacaoCandidatoEvent>());
    }

    private static CancellationTokenSource ExpiringCancellationToken(int msTimeout = 150)
    {
        var timeout = TimeSpan.FromMilliseconds(msTimeout);
        return new CancellationTokenSource(timeout);
    }
}